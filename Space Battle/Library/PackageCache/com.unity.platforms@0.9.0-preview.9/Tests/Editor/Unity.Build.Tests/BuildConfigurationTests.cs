using NUnit.Framework;
using System;
using System.IO;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Tests
{
    class BuildConfigurationTests : BuildTestsBase
    {
        [Test]
        public void CreateAsset()
        {
            const string assetPath = "Assets/" + nameof(BuildConfigurationTests) + BuildConfiguration.AssetExtension;
            Assert.That(BuildConfiguration.CreateAsset(assetPath), Is.Not.Null);
            AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void GetBuildPipeline_IsEqualToPipeline()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.GetBuildPipeline(), Is.EqualTo(pipeline));
        }

        [Test]
        public void GetBuildPipeline_WithoutPipeline_IsNull()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.GetBuildPipeline(), Is.Null);
        }

        [Test]
        public void IsComponentUsed()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));

            Assert.That(config.IsComponentUsed<TestBuildComponentA>(), Is.True);
            Assert.That(config.IsComponentUsed<TestBuildComponentB>(), Is.True);
            Assert.That(config.IsComponentUsed<TestBuildComponentC>(), Is.False);

            Assert.Throws<ArgumentNullException>(() => config.IsComponentUsed(null));
            Assert.Throws<InvalidOperationException>(() => config.IsComponentUsed(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => config.IsComponentUsed(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void CanBuild_IsTrue()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanBuild().Result, Is.True);
        }

        [Test]
        public void CanBuild_WithoutPipeline_IsFalse()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.CanBuild().Result, Is.False);
        }

        [Test]
        public void CanBuild_WhenCannotBuild_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantBuild();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanBuild().Result, Is.False);
        }

        [Test]
        public void Build_Succeeds()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
        }

        [Test]
        public void Build_WithoutPipeline_Fails()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenCannotBuild_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantBuild();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildFails_Fails()
        {
            var pipeline = new TestBuildPipelineBuildFails();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildThrows_Fails()
        {
            var pipeline = new TestBuildPipelineBuildThrows();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void CanRun_IsTrue()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun().Result, Is.True);
        }

        [Test]
        public void CanRun_WithoutBuild_IsFalse()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WithFailedBuild_IsFalse()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WithoutPipeline_IsFalse()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestBuildPipelineComponent>();
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WhenCannotRun_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantRun();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void Run_Succeeds()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.True);
            }
        }

        [Test]
        public void Run_WithoutBuild_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithFailedBuild_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithoutPipeline_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestBuildPipelineComponent>();
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenCannotRun_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantRun();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunFails_Fails()
        {
            var pipeline = new TestBuildPipelineRunFails();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunThrows_Fails()
        {
            var pipeline = new TestBuildPipelineRunThrows();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        class TestMigrationContext : IJsonMigration<TestBuildComponentA>
        {
            public const string k_AssetPath = "Assets/" + nameof(BuildConfigurationTests) + BuildConfiguration.AssetExtension;

            public int Version => 1;

            public TestBuildComponentA Migrate(JsonMigrationContext context)
            {
                context.TryRead<TestBuildComponentA>(out var component);
                if (context.SerializedVersion == 0)
                {
                    var deserializationContext = context.UserData as BuildConfiguration.DeserializationContext;
                    Assert.That(deserializationContext, Is.Not.Null);
                    Assert.That(deserializationContext.AssetPath, Is.EqualTo(k_AssetPath));

                    deserializationContext.Asset.SetComponent<TestBuildComponentB>();
                    Assert.That(deserializationContext.Asset.HasComponent<TestBuildComponentB>(), Is.True);
                }
                return component;
            }
        }

        [Test]
        public void DeserializationContext()
        {
            var migration = new TestMigrationContext();
            JsonSerialization.AddGlobalMigration(migration);

            File.WriteAllText(TestMigrationContext.k_AssetPath, $"{{\"Dependencies\": [], \"Components\": [{{\"$type\": {typeof(TestBuildComponentA).GetAssemblyQualifiedTypeName().DoubleQuotes()}}}]}}");
            AssetDatabase.ImportAsset(TestMigrationContext.k_AssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var asset = BuildConfiguration.LoadAsset(TestMigrationContext.k_AssetPath);
            Assert.That(asset.HasComponent<TestBuildComponentA>(), Is.True);
            Assert.That(asset.HasComponent<TestBuildComponentB>(), Is.True);

            AssetDatabase.DeleteAsset(TestMigrationContext.k_AssetPath);
            JsonSerialization.RemoveGlobalMigration(migration);
        }

        [HideInInspector]
        class ComponentConstruct : IBuildComponent, ICustomBuildComponentConstructor
        {
            public int Integer;
            void ICustomBuildComponentConstructor.Construct(BuildConfiguration.ReadOnly config) => Integer = 255;
        }

        [Test]
        public void BuildComponentConstruct()
        {
            var container = BuildConfiguration.CreateInstance();
            Assert.That(container.GetComponentOrDefault<ComponentConstruct>().Integer, Is.EqualTo(255));

            container.SetComponent<ComponentConstruct>();
            Assert.That(container.GetComponent<ComponentConstruct>().Integer, Is.EqualTo(255));
            Assert.That(container.GetComponentOrDefault<ComponentConstruct>().Integer, Is.EqualTo(255));

            container.SetComponent(new ComponentConstruct());
            Assert.That(container.GetComponent<ComponentConstruct>().Integer, Is.EqualTo(0));
            Assert.That(container.GetComponentOrDefault<ComponentConstruct>().Integer, Is.EqualTo(0));
        }
    }
}
