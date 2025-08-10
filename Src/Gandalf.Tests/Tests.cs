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
}