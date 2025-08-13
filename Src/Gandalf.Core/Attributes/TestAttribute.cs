using System;

namespace Gandalf.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class TestAttribute : Attribute
{
}
