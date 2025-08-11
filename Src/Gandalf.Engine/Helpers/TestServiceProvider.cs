using System;

namespace Gandalf.Engine.Helpers
{
    internal static class TestServiceProvider
    {
#if NET7_0_OR_GREATER
        private static IServiceProvider? _assemblyServiceProvider;
        public static void SetAssemblyServiceProvider(object? provider)
        {
            _assemblyServiceProvider = (IServiceProvider)provider!;
        }

        public static IServiceProvider? GetAssemblyServiceProvider()
        {
            return _assemblyServiceProvider;
        }
#else
        private static IServiceProvider _assemblyServiceProvider;

        public static void SetAssemblyServiceProvider(object provider)
        {
            _assemblyServiceProvider = (IServiceProvider)provider!;
        }

        public static IServiceProvider GetAssemblyServiceProvider()
        {
            return (IServiceProvider)_assemblyServiceProvider;
        }
#endif
    }
}
