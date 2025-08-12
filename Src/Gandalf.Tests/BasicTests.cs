namespace Gandalf.Tests;

using Gandalf.Core.Attributes;
using Gandalf.Core.Models;

public class BasicTests
{
    [Test]
    [Argument(1)]
    public Task SimpleTest(int a)
    {
        return Task.CompletedTask;
    }

    [Test]
    [Argument(1, 2, 3)]
    [Argument(3, 4, 7)]
    public Task SimpleTest_WithParameter_Ints(int val1, int val2, int result)
    {
        Console.WriteLine($"Testing addition: {val1} + {val2} = {result}");
        if (val1 + val2 != result)
            throw new Exception("Test failed");

        return Task.CompletedTask;
    }

    [Test]
    [Argument("Jeff")]
    public Task SimpleTest_WithParameter_Strings(string value)
    {
        Console.WriteLine($"My name is {value}");
        if (value != "Jeff")
            throw new Exception("Test failed");

        return Task.CompletedTask;
    }

    [Test]
    public async Task SimpleTest_WithDelay()
    {
        await Task.Delay(100);
    }
}
