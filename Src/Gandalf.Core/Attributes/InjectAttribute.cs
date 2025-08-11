using System;

namespace Gandalf.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
