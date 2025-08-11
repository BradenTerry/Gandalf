using System;

namespace Gandalf.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class ArgumentAttribute : Attribute
    {
        public object[] Args { get; }

        public ArgumentAttribute(params object[] args)
        {
            Args = args;
        }
    }
}
