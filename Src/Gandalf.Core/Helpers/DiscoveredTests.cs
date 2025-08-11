using System;
using System.Collections.Generic;
using Gandalf.Core.Models;

namespace Gandalf.Core.Helpers
{
    public static partial class DiscoveredTests
    {
        private readonly static List<DiscoveredTest> _allTests = new List<DiscoveredTest>();
        internal static IReadOnlyList<DiscoveredTest> All => _allTests.AsReadOnly();

        public static void Register(DiscoveredTest test)
        {
            _allTests.Add(test);
        }
    }
}
