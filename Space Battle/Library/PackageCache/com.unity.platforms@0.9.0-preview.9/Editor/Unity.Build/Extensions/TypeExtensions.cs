using System;
using System.Reflection;

namespace Unity.Build
{
    internal static class TypeExtensions
    {
        public static string GetAssemblyQualifiedTypeName(this Type type) => $"{type}, {type.Assembly.GetName().Name}";
        public static bool HasAttribute<T>(this Type type) where T : Attribute => type.GetCustomAttribute<T>() != null;
    }
}
