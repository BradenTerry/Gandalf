using System;
using System.Collections.Generic;
using System.IO;

namespace Gandalf.Core.Models
{
    public interface ITestContext
    {
        string TestId { get; }
        string TestName { get; }
        string TestAssembly { get; }
        TextWriter TextWriter { get; }
        void AddAttachment(string filePath);
    }

    internal class TestContext : ITestContext
    {
        public string TestId { get; }
        public string TestName { get; }
        public string TestAssembly { get; }
        public TextWriter TextWriter { get; }
        public List<string> Attachments { get; }

        public TestContext(string testId, string testName, string testAssembly, TextWriter textWriter)
        {
            TestId = testId;
            TestName = testName;
            TestAssembly = testAssembly;
            TextWriter = textWriter;
        }

        public void AddAttachment(string filePath)
        {
            Attachments.Add(filePath);
        }
    }
}
