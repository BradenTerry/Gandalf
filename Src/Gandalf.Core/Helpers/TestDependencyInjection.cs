using System;
using System.Collections.Generic;

namespace Gandalf.Core.Helpers
{
    /// <summary>
    /// Simple dependency injection container for test classes
    /// </summary>
    public static class TestDependencyInjection
    {
        private static readonly Dictionary<Type, object> _registeredTypes = new Dictionary<Type, object>();

        /// <summary>
        /// Register a dependency of the specified type
        /// </summary>
        public static void RegisterDependency(Type type)
        {
            if (!_registeredTypes.ContainsKey(type))
            {
                _registeredTypes.Add(type, Activator.CreateInstance(type));
            }
        }
        
        /// <summary>
        /// Register a dependency with an existing instance
        /// </summary>
        public static void RegisterDependency<T>(T instance)
        {
            var type = typeof(T);
            if (_registeredTypes.ContainsKey(type))
                return;

            _registeredTypes.Add(type, instance);
        }
        
        /// <summary>
        /// Get a registered dependency of the specified type
        /// </summary>
        public static T GetDependency<T>() where T : class
        {
            var type = typeof(T);
            if (_registeredTypes.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }
            
            // Auto-register if not found
            var newInstance = Activator.CreateInstance<T>();
            RegisterDependency(newInstance);
            return newInstance;
        }
        
        /// <summary>
        /// Clear all registered dependencies
        /// </summary>
        public static void Reset()
        {
            _registeredTypes.Clear();
        }
    }
}
