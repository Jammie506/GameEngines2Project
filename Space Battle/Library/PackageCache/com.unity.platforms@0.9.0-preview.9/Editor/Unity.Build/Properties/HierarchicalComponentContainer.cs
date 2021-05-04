using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// Base class that stores a set of hierarchical components by type.
    /// Other containers can be added as dependencies to get inherited or overridden components.
    /// </summary>
    /// <typeparam name="TContainer">Type of the component container.</typeparam>
    /// <typeparam name="TComponent">Components base type.</typeparam>
    public abstract class HierarchicalComponentContainer<TContainer, TComponent> : ScriptableObjectPropertyContainer<TContainer>
        where TContainer : HierarchicalComponentContainer<TContainer, TComponent>
    {
        [CreateProperty] internal readonly List<LazyLoadReference<TContainer>> Dependencies = new List<LazyLoadReference<TContainer>>();
        [CreateProperty] internal readonly List<TComponent> Components = new List<TComponent>();

        /// <summary>
        /// Create a new instance that duplicates the specified container.
        /// </summary>
        /// <param name="mutator">Optional mutator that can be used to modify the asset.</param>
        /// <returns>The new asset instance.</returns>
        public static TContainer CreateInstance(TContainer container)
        {
            var instance = CreateInstance<TContainer>();
            foreach (var component in container.Components)
            {
                instance.SetComponent(component.GetType(), component);
            }
            foreach (var dependency in container.Dependencies)
            {
                instance.Dependencies.Add(dependency);
            }
            return instance;
        }

        /// <summary>
        /// Determine if a <see cref="Type"/> component is stored in this container or its dependencies.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public bool HasComponent(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return HasComponentOnSelf(type) || HasComponentOnDependency(type);
        }

        /// <summary>
        /// Determine if a <typeparamref name="T"/> component is stored in this container or its dependencies.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool HasComponent<T>() where T : TComponent => HasComponent(typeof(T));

        /// <summary>
        /// Determine if a <see cref="Type"/> component is inherited from a dependency.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public bool IsComponentInherited(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return !HasComponentOnSelf(type) && HasComponentOnDependency(type);
        }

        /// <summary>
        /// Determine if a <typeparamref name="T"/> component is inherited from a dependency.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool IsComponentInherited<T>() where T : TComponent => IsComponentInherited(typeof(T));

        /// <summary>
        /// Determine if a <see cref="Type"/> component overrides a dependency.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2020-11-30)")]
        public bool IsComponentOverridden(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return HasComponentOnSelf(type) && HasComponentOnDependency(type);
        }

        /// <summary>
        /// Determine if a <typeparamref name="T"/> component overrides a dependency.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2020-11-30)")]
        public bool IsComponentOverridden<T>() where T : TComponent => IsComponentOverriding(typeof(T));

        /// <summary>
        /// Determines if component overrides a dependency.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return HasComponentOnSelf(type) && HasComponentOnDependency(type);
        }

        /// <summary>
        /// Determines if component overrides a dependency.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding<T>() where T : TComponent => IsComponentOverriding(typeof(T));

        /// <summary>
        /// Get the value of a <see cref="Type"/> component.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public TComponent GetComponent(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            if (!TryGetComponent(type, out var value))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' not found.");
            }
            return value;
        }

        /// <summary>
        /// Get the value of a <typeparamref name="T"/> component.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public T GetComponent<T>() where T : TComponent => (T)GetComponent(typeof(T));

        /// <summary>
        /// Try to get the value of a <see cref="Type"/> component.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <param name="value">Out value of the component.</param>
        public bool TryGetComponent(Type type, out TComponent value)
        {
            if (!TryGetDerivedTypeFromBaseType(type, out type) ||
                !(HasComponentOnSelf(type) || HasComponentOnDependency(type)) ||
                !TypeConstruction.TryConstruct<TComponent>(type, out var result))
            {
                value = default;
                return false;
            }

            for (var i = 0; i < Dependencies.Count; ++i)
            {
                var dependency = Dependencies[i].asset;
                if (dependency == null || !dependency)
                {
                    continue;
                }

                if (dependency.TryGetComponent(type, out var component))
                {
                    CopyComponent(ref result, ref component);
                }
            }

            for (var i = 0; i < Components.Count; ++i)
            {
                var component = Components[i];
                if (component.GetType() == type)
                {
                    CopyComponent(ref result, ref component);
                    break;
                }
            }

            value = result;
            return true;
        }

        /// <summary>
        /// Try to get the value of a <typeparamref name="T"/> component.
        /// </summary>
        /// <param name="value">Out value of the component.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool TryGetComponent<T>(out T value) where T : TComponent
        {
            if (TryGetComponent(typeof(T), out var result))
            {
                value = (T)result;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get the value of a <see cref="Type"/> component if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <returns>The component value.</returns>
        public TComponent GetComponentOrDefault(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            if (!TryGetComponent(type, out var value))
            {
                var component = TypeConstruction.Construct<TComponent>(type);
                OnComponentConstruct(ref component);
                return component;
            }
            return value;
        }

        /// <summary>
        /// Get the value of a <typeparamref name="T"/> component if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>The component value.</returns>
        public T GetComponentOrDefault<T>() where T : TComponent => (T)GetComponentOrDefault(typeof(T));

        /// <summary>
        /// Get the source container from which the component value is coming from.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
        /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
        public TContainer GetComponentSource(Type type, bool dependenciesOnly = false)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            if (!TryGetDerivedTypeFromBaseType(type, out type))
            {
                return null;
            }

            if (!dependenciesOnly && HasComponentOnSelf(type))
            {
                return (TContainer)this;
            }

            for (var i = Dependencies.Count - 1; i >= 0; --i)
            {
                var dependency = Dependencies[i].asset;
                if (dependency == null || !dependency)
                {
                    continue;
                }

                var source = dependency.GetComponentSource(type, false);
                if (source != null)
                {
                    return source;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the source container from which the component value is coming from.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
        /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
        public TContainer GetComponentSource<T>(bool dependenciesOnly = false) where T : TComponent => GetComponentSource(typeof(T), dependenciesOnly);

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies.
        /// </summary>
        /// <returns>List of components.</returns>
        public IEnumerable<TComponent> GetComponents()
        {
            var lookup = new Dictionary<Type, TComponent>();
            foreach (var dependency in GetDependencies())
            {
                foreach (var component in dependency.Components)
                {
                    lookup[component.GetType()] = CopyComponent(component);
                }
            }

            foreach (var component in Components)
            {
                lookup[component.GetType()] = CopyComponent(component);
            }

            return lookup.Values;
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <see cref="Type"/>.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <returns>List of components.</returns>
        public IEnumerable<TComponent> GetComponents(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return GetComponents().Where(component => type.IsAssignableFrom(component.GetType()));
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <returns>List of components.</returns>
        public IEnumerable<T> GetComponents<T>() where T : TComponent => GetComponents(typeof(T)).Cast<T>();

        /// <summary>
        /// Get a flatten list of all component types from this container and its dependencies.
        /// </summary>
        /// <returns>List of component types.</returns>
        public IEnumerable<Type> GetComponentTypes()
        {
            var types = new HashSet<Type>();
            foreach (var dependency in GetDependencies())
            {
                foreach (var component in dependency.Components)
                {
                    types.Add(component.GetType());
                }
            }

            foreach (var component in Components)
            {
                types.Add(component.GetType());
            }

            return types;
        }

        /// <summary>
        /// Set the value of a <see cref="Type"/> component on this container.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <param name="value">Value of the component to set.</param>
        public void SetComponent(Type type, TComponent value)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            if (type.IsInterface || type.IsAbstract)
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be interface or abstract.");
            }

            for (var i = 0; i < Components.Count; ++i)
            {
                if (Components[i].GetType() == type)
                {
                    Components[i] = CopyComponent(value);
                    return;
                }
            }

            Components.Add(CopyComponent(value));
        }

        /// <summary>
        /// Set the value of a <typeparamref name="T"/> component on this container.
        /// </summary>
        /// <param name="value">Value of the component to set.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>(T value) where T : TComponent => SetComponent(typeof(T), value);

        /// <summary>
        /// Set the value of a <see cref="Type"/> component on this container using an instance created using <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <param name="type">Type of the component.</param>
        public void SetComponent(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            if (type.IsInterface || type.IsAbstract)
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be interface or abstract.");
            }

            var component = TypeConstruction.Construct<TComponent>(type);
            OnComponentConstruct(ref component);
            SetComponent(type, component);
        }

        /// <summary>
        /// Set the value of a <typeparamref name="T"/> component on this container using an instance created using <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>() where T : TComponent => SetComponent(typeof(T));

        /// <summary>
        /// Remove a <see cref="Type"/> component from this container.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public bool RemoveComponent(Type type)
        {
            CheckComponentTypeAndThrowIfInvalid(type);
            return Components.RemoveAll(c => type.IsAssignableFrom(c.GetType())) > 0;
        }

        /// <summary>
        /// Remove all <typeparamref name="T"/> components from this container.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool RemoveComponent<T>() where T : TComponent => RemoveComponent(typeof(T));

        /// <summary>
        /// Remove all components from this container.
        /// </summary>
        public void ClearComponents() => Components.Clear();

        /// <summary>
        /// Determine if a dependency exist in this container or its dependencies.
        /// </summary>
        /// <param name="dependency">The dependency to search.</param>
        /// <returns><see langword="true"/> if the dependency is found, <see langword="false"/> otherwise.</returns>
        public bool HasDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
            {
                return false;
            }

            return GetDependencies().Contains(dependency);
        }

        /// <summary>
        /// Add a dependency to this container.
        /// Circular dependencies or dependencies on self are not allowed.
        /// </summary>
        /// <param name="dependency">The dependency to add.</param>
        /// <returns><see langword="true"/> if the dependency was added, <see langword="false"/> otherwise.</returns>
        public bool AddDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (dependency == this || HasDependency(dependency) || dependency.HasDependency(this as TContainer))
            {
                return false;
            }

            Dependencies.Add(dependency.GetInstanceID());
            return true;
        }

        /// <summary>
        /// Get a flatten list of all dependencies recursively from this container and its dependencies.
        /// </summary>
        /// <returns>List of dependencies.</returns>
        public IEnumerable<TContainer> GetDependencies()
        {
            var dependencies = new List<TContainer>();
            for (var i = 0; i < Dependencies.Count; ++i)
            {
                var dependency = Dependencies[i].asset;
                if (dependency == null || !dependency || dependency == this)
                {
                    continue;
                }
                dependencies.AddRange(dependency.GetDependencies());
                dependencies.Add(dependency);
            }
            return dependencies;
        }

        /// <summary>
        /// Remove a dependency from this container.
        /// </summary>
        /// <param name="dependency">The dependency to remove.</param>
        public bool RemoveDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
            {
                throw new ArgumentNullException(nameof(dependency));
            }
            return Dependencies.Remove(dependency.GetInstanceID());
        }

        /// <summary>
        /// Remove all dependencies from this container.
        /// </summary>
        public void ClearDependencies() => Dependencies.Clear();

        /// <summary>
        /// A read-only wrapper for this container, which does not expose methods that modify the container.
        /// If changes are made to the underlying container, the read-only wrapper reflects those changes.
        /// </summary>
        public class ReadOnly
        {
            readonly HierarchicalComponentContainer<TContainer, TComponent> m_Container;

            internal ReadOnly(HierarchicalComponentContainer<TContainer, TComponent> container)
            {
                m_Container = container;
            }

            /// <summary>
            /// Determine if a <see cref="Type"/> component is stored in this container or its dependencies.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            public bool HasComponent(Type type) => m_Container.HasComponent(type);

            /// <summary>
            /// Determine if a <typeparamref name="T"/> component is stored in this container or its dependencies.
            /// </summary>
            /// <typeparam name="T">Type of the component.</typeparam>
            public bool HasComponent<T>() where T : TComponent => m_Container.HasComponent<T>();

            /// <summary>
            /// Determine if a <see cref="Type"/> component is inherited from a dependency.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            public bool IsComponentInherited(Type type) => m_Container.IsComponentInherited(type);

            /// <summary>
            /// Determine if a <typeparamref name="T"/> component is inherited from a dependency.
            /// </summary>
            /// <typeparam name="T">Type of the component.</typeparam>
            public bool IsComponentInherited<T>() where T : TComponent => m_Container.IsComponentInherited<T>();

            /// <summary>
            /// Determine if a <see cref="Type"/> component overrides a dependency.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2020-11-30)")]
            public bool IsComponentOverridden(Type type) => m_Container.IsComponentOverriding(type);

            /// <summary>
            /// Determine if a <typeparamref name="T"/> component overrides a dependency.
            /// </summary>
            /// <typeparam name="T">Type of the component.</typeparam>
            [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2020-11-30)")]
            public bool IsComponentOverridden<T>() where T : TComponent => m_Container.IsComponentOverriding<T>();

            /// <summary>
            /// Determines if component overrides a dependency.
            /// </summary>
            /// <param name="type">The component type.</param>
            /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise.</returns>
            public bool IsComponentOverriding(Type type) => m_Container.IsComponentOverriding(type);

            /// <summary>
            /// Determines if component overrides a dependency.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns><see langword="true"/> if component overrides a dependency, <see langword="false"/> otherwise.</returns>
            public bool IsComponentOverriding<T>() where T : TComponent => m_Container.IsComponentOverriding<T>();

            /// <summary>
            /// Get the value of a <see cref="Type"/> component.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            public TComponent GetComponent(Type type) => m_Container.GetComponent(type);

            /// <summary>
            /// Get the value of a <typeparamref name="T"/> component.
            /// </summary>
            /// <typeparam name="T">Type of the component.</typeparam>
            public T GetComponent<T>() where T : TComponent => m_Container.GetComponent<T>();

            /// <summary>
            /// Try to get the value of a <see cref="Type"/> component.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            /// <param name="value">Out value of the component.</param>
            public bool TryGetComponent(Type type, out TComponent value) => m_Container.TryGetComponent(type, out value);

            /// <summary>
            /// Try to get the value of a <typeparamref name="T"/> component.
            /// </summary>
            /// <param name="value">Out value of the component.</param>
            /// <typeparam name="T">Type of the component.</typeparam>
            public bool TryGetComponent<T>(out T value) where T : TComponent => m_Container.TryGetComponent(out value);

            /// <summary>
            /// Get the value of a <see cref="Type"/> component if found.
            /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
            /// The container is not modified.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            /// <returns>The component value.</returns>
            public TComponent GetComponentOrDefault(Type type) => m_Container.GetComponentOrDefault(type);

            /// <summary>
            /// Get the value of a <typeparamref name="T"/> component if found.
            /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
            /// The container is not modified.
            /// </summary>
            /// <typeparam name="T">Type of the component.</typeparam>
            /// <returns>The component value.</returns>
            public T GetComponentOrDefault<T>() where T : TComponent => m_Container.GetComponentOrDefault<T>();

            /// <summary>
            /// Get a flatten list of all components recursively from this container and its dependencies.
            /// </summary>
            /// <returns>List of components.</returns>
            public IEnumerable<TComponent> GetComponents() => m_Container.GetComponents();

            /// <summary>
            /// Get a flatten list of all components recursively from this container and its dependencies, that matches <see cref="Type"/>.
            /// </summary>
            /// <param name="type"><see cref="Type"/> of the component.</param>
            /// <returns>List of components.</returns>
            public IEnumerable<TComponent> GetComponents(Type type) => m_Container.GetComponents(type);

            /// <summary>
            /// Get a flatten list of all components recursively from this container and its dependencies, that matches <see cref="Type"/>.
            /// </summary>
            /// <typeparam name="T">Type of the components.</typeparam>
            /// <returns>List of components.</returns>
            public IEnumerable<T> GetComponents<T>() where T : TComponent => m_Container.GetComponents<T>();

            /// <summary>
            /// Get a flatten list of all component types from this container and its dependencies.
            /// </summary>
            /// <returns>List of component types.</returns>
            public IEnumerable<Type> GetComponentTypes() => m_Container.GetComponentTypes();
        }

        /// <summary>
        /// Returns a read-only wrapper for this container.
        /// </summary>
        /// <returns></returns>
        public ReadOnly AsReadOnly() => new ReadOnly(this);

        protected override void Reset()
        {
            base.Reset();
            Dependencies.Clear();
            Components.Clear();
        }

        protected override void Sanitize()
        {
            base.Sanitize();
            // Note: We do not remove null dependencies because we want them to appear on the UI
            Components.RemoveAll(component => component == null);
        }

        internal static IEnumerable<Type> GetAvailableTypes(Func<Type, bool> filter = null)
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<TComponent>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (filter != null && !filter(type))
                {
                    continue;
                }

                yield return type;
            }
        }

        internal static void CheckComponentTypeAndThrowIfInvalid(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type == typeof(object))
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be 'object'.");
            }

            if (!typeof(TComponent).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' must derive from '{typeof(TComponent).FullName}'.");
            }
        }

        protected bool HasComponentOnSelf(Type type) => Components.Any(component => type.IsAssignableFrom(component.GetType()));

        protected bool HasComponentOnDependency(Type type) => GetDependencies().Any(dependency => dependency.HasComponentOnSelf(type));

        bool TryGetDerivedTypeFromBaseType(Type baseType, out Type value)
        {
            if (baseType == null || baseType == typeof(object) || !typeof(TComponent).IsAssignableFrom(baseType))
            {
                value = default;
                return false;
            }

            if (!baseType.IsInterface && !baseType.IsAbstract)
            {
                value = baseType;
                return true;
            }

            foreach (var dependency in GetDependencies())
            {
                foreach (var component in dependency.Components)
                {
                    var componentType = component.GetType();
                    if (baseType.IsAssignableFrom(componentType))
                    {
                        value = componentType;
                        return true;
                    }
                }
            }

            foreach (var component in Components)
            {
                var componentType = component.GetType();
                if (baseType.IsAssignableFrom(componentType))
                {
                    value = componentType;
                    return true;
                }
            }

            value = baseType;
            return false;
        }

        protected virtual void OnComponentConstruct(ref TComponent component) { }

        static TComponent CopyComponent(TComponent value)
        {
            var visitor = new CopyVisitor<TComponent>(ref value);
            PropertyContainer.Visit(ref value, visitor);
            return visitor.Result;
        }

        static void CopyComponent(ref TComponent result, ref TComponent value)
        {
            var visitor = new CopyVisitor<TComponent>(ref value);
            PropertyContainer.Visit(ref value, visitor);
            result = visitor.Result;
        }

        class CopyVisitor<T> : PropertyVisitor
        {
            T m_DstContainer;

            public T Result => m_DstContainer;

            public CopyVisitor(ref T srcContainer)
            {
                m_DstContainer = TypeConstruction.Construct<T>(srcContainer.GetType());
            }

            protected override void VisitProperty<TSrcContainer, TSrcValue>(Property<TSrcContainer, TSrcValue> property, ref TSrcContainer container, ref TSrcValue value)
            {
                PropertyContainer.TrySetValue(ref m_DstContainer, property.Name, value);
            }

            protected override void VisitList<TSrcContainer, TSrcList, TSrcElement>(Property<TSrcContainer, TSrcList> property, ref TSrcContainer container, ref TSrcList value)
            {
                TSrcList list;
                if (typeof(TSrcList).IsArray)
                {
                    list = TypeConstruction.ConstructArray<TSrcList>(value.Count);
                    for (var i = 0; i < value.Count; ++i)
                    {
                        list[i] = value[i];
                    }
                }
                else
                {
                    list = TypeConstruction.Construct<TSrcList>();
                    foreach (var item in value)
                    {
                        list.Add(item);
                    }
                }
                base.VisitList<TSrcContainer, TSrcList, TSrcElement>(property, ref container, ref list);
            }

            protected override void VisitSet<TSrcContainer, TSrcSet, TSrcValue>(Property<TSrcContainer, TSrcSet> property, ref TSrcContainer container, ref TSrcSet value)
            {
                var set = TypeConstruction.Construct<TSrcSet>();
                foreach (var item in value)
                {
                    set.Add(item);
                }
                base.VisitSet<TSrcContainer, TSrcSet, TSrcValue>(property, ref container, ref set);
            }

            protected override void VisitDictionary<TSrcContainer, TSrcDictionary, TSrcKey, TSrcValue>(Property<TSrcContainer, TSrcDictionary> property, ref TSrcContainer container, ref TSrcDictionary value)
            {
                var dictionary = TypeConstruction.Construct<TSrcDictionary>();
                foreach (var item in value)
                {
                    dictionary.Add(item);
                }
                base.VisitDictionary<TSrcContainer, TSrcDictionary, TSrcKey, TSrcValue>(property, ref container, ref dictionary);
            }
        }
    }
}
