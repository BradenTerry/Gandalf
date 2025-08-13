using System;

namespace Gandalf.Core.Attributes;

/// <summary>
/// Specifies a group for sharing scoped instances across properties/classes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ScopedGroupAttribute : Attribute
{
    public string GroupName { get; }
    public ScopedGroupAttribute(string groupName)
    {
        GroupName = groupName;
    }
}
