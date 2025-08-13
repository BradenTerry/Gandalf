using System;

namespace Gandalf.Core.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class InjectAttribute : Attribute
{
    public InstanceType InstanceType { get; }

    public InjectAttribute(InstanceType instanceType = InstanceType.Transient)
    {
        InstanceType = instanceType;
    }
}

public enum InstanceType
{
    /// <summary>
    /// Created each time it is requested.
    /// </summary>
    Transient,
    /// <summary>
    /// Created once per class.
    /// </summary>
    Scoped,
    /// <summary>
    /// Created once and shared throughout the application's lifetime.
    /// </summary>
    Singleton
}
