namespace Gandalf.Tests;

using Gandalf.Core.Attributes;

public class Tests
{
    [Test]
    public async Task Test()
    {
        await Task.Delay(500);
        // Should pass
    }

    [Test]
    [Argument(1)]
    public async Task Test_WithParameter()
    {
        await Task.Delay(500);
        // Should pass
    }
}