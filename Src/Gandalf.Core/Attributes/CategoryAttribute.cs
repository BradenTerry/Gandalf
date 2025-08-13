using System;

namespace Gandalf.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CategoryAttribute : Attribute
    {
        public string Name { get; }
        public CategoryAttribute(string name)
        {
            Name = name;
        }
    }
}
