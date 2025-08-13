using System;

namespace Gandalf.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
    public string Reason { get; }
        public IgnoreAttribute() { }
        public IgnoreAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
