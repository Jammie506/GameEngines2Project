using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Options;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

public class NamedILPostProcessorWrapper
{
    public string Name { get; }
    public ILPostProcessor ILPostProcessor { get; }

    public NamedILPostProcessorWrapper(string name, ILPostProcessor ilpostProcessor)
    {
        Name = name;
        ILPostProcessor = ilpostProcessor;
    }
}

public class ILPostProcessorRunner
{
    static NamedILPostProcessorWrapper[] NamedILPostProcessors = Array.Empty<NamedILPostProcessorWrapper>();
    static CompiledAssembly InputAssembly;
    static string[] AssemblyFolderPaths;
    static string OutputAsmPath;
    static string OutputPdbPath;

    /// <summary>
    /// Commandline Options
    /// </summary>
    class Options
    {
        public string AssemblyPath { get; set; }

        public string OutputDir { get; set; }

        public List<string> ILPostProcessorPaths { get; set; } = new List<string>();

        public List<string> AssemblyFolderPaths { get; set; } = new List<string>();

        public List<string> ReferenceAssemblyPaths { get; set; } = new List<string>();

        public List<string> Defines { get; set; } = new List<string>();
    }

    /// <summary>
    /// In-memory representation of a built assembly, with full path strings to the assemblies it references
    /// </summary>
    public class CompiledAssembly : ICompiledAssembly
    {
        public string Name { get; }
        public string[] References { get; }
        public string[] Defines { get; }
        public InMemoryAssembly InMemoryAssembly { get; set; }

        public CompiledAssembly(string asmPath, string[] referencePaths, string[] defines)
        {
            var peData = File.ReadAllBytes(asmPath);

            byte[] pdbData = null;
            var pdbPath = Path.ChangeExtension(asmPath, "pdb");
            if (File.Exists(pdbPath))
                pdbData = File.ReadAllBytes(pdbPath);

            Name = Path.GetFileNameWithoutExtension(asmPath);
            References = referencePaths;
            Defines = defines;
            InMemoryAssembly = new InMemoryAssembly(peData, pdbData);
        }

        public void Save()
        {
            File.WriteAllBytes(OutputAsmPath, InMemoryAssembly.PeData);

            if (InMemoryAssembly.PdbData != null)
                File.WriteAllBytes(OutputPdbPath, InMemoryAssembly.PdbData);
        }
    }

    static bool ProcessArgs(string[] args)
    {
        bool success = true;

        try
        {
            var options = new Options();
            //args = args.SelectMany(a => a.Split(' ')).ToArray();

            OptionSet optionSet = new OptionSet()
                .Add("a|assembly=", a =>
            {
                options.AssemblyPath = a.Replace("\"", "");
            })
                .Add("o|outputDir=", a =>
                {
                    options.OutputDir = a.Replace("\"", "");
                })
                .Add("p|processors=", a => options.ILPostProcessorPaths.AddRange(a.Replace("\"", "").Split(',')))
                .Add("f|assemblyFolders=", a => options.AssemblyFolderPaths.AddRange(a.Replace("\"", "").Split(',')))
                .Add("r|assemblyReferences=", a => options.ReferenceAssemblyPaths.AddRange(a.Replace("\"", "").Split(',')))
                .Add("d|defines=", a => options.Defines.AddRange(a.Split(',')));

            optionSet.Parse(args);



            AssemblyFolderPaths = options.AssemblyFolderPaths.ToArray();

            NamedILPostProcessors = options.ILPostProcessorPaths
                .Select(p => Assembly.LoadFrom(p))
                .SelectMany(asm => asm.GetTypes().Where(t => typeof(ILPostProcessor).IsAssignableFrom(t)))
                .Select(t => new NamedILPostProcessorWrapper(t.FullName, (ILPostProcessor)Activator.CreateInstance(t)))
                .ToArray();

            OutputAsmPath = Path.Combine(options.OutputDir, Path.GetFileName(options.AssemblyPath));
            OutputPdbPath = Path.ChangeExtension(OutputAsmPath, "pdb");
            InputAssembly = new CompiledAssembly(options.AssemblyPath, options.ReferenceAssemblyPaths.ToArray(), options.Defines?.ToArray() ?? Array.Empty<string>());
        }
        catch (Exception e)
        {
            var rtle = e as ReflectionTypeLoadException;

            if (rtle == null)
            {
                throw;
            }

            Console.Error.WriteLine("ILPostProcessorRunner caught the following exception while processing arguments:\n" + e);

            if (rtle != null)
            {
                foreach (Exception inner in rtle.LoaderExceptions)
                {
                    Console.Error.WriteLine(inner);
                }
            }

            success = false;
        }

        return success;
    }

    public static NamedILPostProcessorWrapper[] SortILPostProcessors()
    {
        // Sort by name to ensure we have some determinism on how we are processing assemblies should
        // two ILPostProcessors potentially conflict. We will need to likely add a more structured ordering later
        var sortedList = new List<NamedILPostProcessorWrapper>(NamedILPostProcessors);
        sortedList.Sort((a, b) => a.Name.CompareTo(b.Name));

        return sortedList.ToArray();
    }

    public static void RunILPostProcessors(NamedILPostProcessorWrapper[] ilpostProcessors)
    {
        foreach (var namedProcessor in ilpostProcessors)
        {
            using (new Marker($"ILPostProcessor '{namedProcessor.Name}' on {InputAssembly.Name}.dll"))
            {
                var processor = namedProcessor.ILPostProcessor;

                if (!processor.WillProcess(InputAssembly))
                    continue;

                var result = processor.Process(InputAssembly);
                if (result == null)
                    continue;

                if (result.InMemoryAssembly != null)
                {
                    InputAssembly.InMemoryAssembly = result.InMemoryAssembly;
                }

                if (result.Diagnostics != null)
                {
                    HandleDiagnosticMessages(processor.GetType().Name, InputAssembly.Name, result.Diagnostics);
                }
            }
        }
    }

    public static void HandleDiagnosticMessages(string ilppName, string asmName, List<DiagnosticMessage> messageList)
    {
        bool hasError = false;

        foreach (var diagMsg in messageList)
        {
            bool isError = false;
            if (diagMsg.DiagnosticType == DiagnosticType.Error)
            {
                hasError = true;
                isError = true;
            }

            var message = $"{diagMsg.File}({diagMsg.Line},{diagMsg.Column}): {(isError ? "error" : "warning")} {diagMsg.MessageData}";
            Console.Error.WriteLine(message);
        }

        if (hasError)
        {
            throw new Exception($"ILPostProcessorRunner '{ilppName}' had a fatal error processing '{asmName}'\n " +
                $"Refer to diagnostic messages above for more details. \n" +
                $"Exiting..."
            );
        }
    }

    static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyFilename = new AssemblyName(args.Name).Name + ".dll";

        var domainAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name.Equals(new AssemblyName(args.Name).Name));

        if (domainAssembly != null)
        {
            return domainAssembly;
        }

        foreach (var folderPath in AssemblyFolderPaths)
        {
            string assemblyPath = Path.Combine(folderPath, assemblyFilename);

            if (!File.Exists(assemblyPath))
                continue;

            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            return assembly;
        }

        return null;
    }

    static string[] SplitBySpace(string input)
    {
        return YieldArguments(input).ToArray();
    }

    static IEnumerable<string> YieldArguments(string input)
    {
        var t= input.Length;
        var arg = new StringBuilder();
        for (int i = 0; i < t; i++)
        {
            char c = input[i];

            if (c == '"' || c == '\'')
            {
                char end = c;

                for (i++; i < t; i++)
                {
                    c = input[i];

                    if (c == end)
                        break;
                    arg.Append(c);
                }
            }
            else if (c == ' ')
            {
                if (arg.Length > 0)
                {
                    yield return arg.ToString();
                    arg.Length = 0;
                }
            }
            else
                arg.Append(c);
        }
    }

    public static int Main(string[] args)
    {
        string[] arguments = args;

        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

        using (new Marker("ILPostProcessorRunner"))
        {
            try
            {
                
                if (args[0].StartsWith("@"))
                {
                    var responseFilePath = args[0].Substring(1);
                    arguments = SplitBySpace(File.ReadAllText(responseFilePath));
                }

                if (!ProcessArgs(arguments))
                    return 2;

                var sortedILPostProcessors = SortILPostProcessors();

                RunILPostProcessors(sortedILPostProcessors);
                // Write out our Input Assembly, processed or not (in which case it's just a copy)
                InputAssembly.Save();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                var argsString = string.Join("\n", args);
                Console.WriteLine($"ILPostPostRunner was started with arguments:\n{argsString}");

                return 1;
            }
        }

        return 0;
    }

    struct Marker : IDisposable
    {
        System.Diagnostics.Stopwatch Stopwatch;
        string Text;
        public Marker(string text)
        {
            Text = text;
            Console.WriteLine($"- Starting {Text}");
            Stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            Console.WriteLine($"- Finished {Text} in {Stopwatch.Elapsed.TotalSeconds:0.######} seconds");
        }
    }
}
