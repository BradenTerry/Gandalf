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
    [Argument(1, 2)]
    [Argument(1, 2)]
    [Argument(1, 2)]
    public async Task Test_WithParameter(int value, int val1)
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }

    [Test]
    [Argument(1, 2)]
    [Argument(1, 2)]
    [Argument(1, 2)]
    public async Task Test_WithParameter2(int value, int val1)
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }

    [Test]
    public async Task Test_1()
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }

    [Test]
    public async Task Test_56()
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }

    [Test]
    public async Task Test_2()
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }
    
    [Test]
    public async Task Test_3()
    {
        Console.WriteLine("Testing...");
        await Task.Delay(500);
        // Should pass
    }
}