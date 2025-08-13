using Gandalf.Core.Attributes;
using System.Threading.Tasks;

namespace Gandalf.Tests
{
    [Category("Integration")]
    public class CategoryAndIgnoreTests
    {
        [Test]
        [Category("Unit")]
        [Category("Fast")]
        public async Task TestWithMultipleCategories()
        {
            // This test has categories: Integration (from class), Unit, Fast (from method)
            await Task.Delay(1);
        }

        [Test]
        [Ignore("Not implemented yet")]
        public async Task IgnoredTestWithReason()
        {
            // This test should be skipped with reason
            await Task.Delay(1);
        }

        [Test]
        [Ignore]
        public async Task IgnoredTestWithoutReason()
        {
            // This test should be skipped with default reason
            await Task.Delay(1);
        }

        [Test]
        [Category("Slow")]
        public async Task SlowTest()
        {
            // This test has categories: Integration (from class), Slow (from method)
            await Task.Delay(1);
        }
    }

    [Ignore("Entire class is ignored")]
    public class IgnoredTestClass
    {
        [Test]
        public async Task TestInIgnoredClass()
        {
            // This test should be skipped because the class is ignored
            await Task.Delay(1);
        }
    }
}
