using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Gandalf.Core
{
    public class AsyncLocalTextWriter : TextWriter
    {
        public static readonly AsyncLocal<StringWriter> Current = new AsyncLocal<StringWriter>();

        public override string ToString()
        {
            return Current.Value?.ToString();
        }

        public override Encoding Encoding => Current.Value?.Encoding ?? Encoding.Default;

        public override void Write(char value)
        {
            (Current.Value ?? Null).Write(value);
        }

        public override void Write(string value)
        {
            (Current.Value ?? Null).Write(value);
        }

        public override void WriteLine(string value)
        {
            (Current.Value ?? Null).WriteLine(value);
        }

        public override void Flush()
        {
            Current.Value?.Flush();
        }
    }
}
