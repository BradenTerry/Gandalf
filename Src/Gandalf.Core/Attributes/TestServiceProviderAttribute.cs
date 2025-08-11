using System;

namespace Gandalf.Core.Attributes
{
#if CSHARP_11_0_OR_GREATER
    public sealed class TestServiceProviderAttribute<T> : TestServiceProviderAttributeBase
        where T : class, IServiceProvider
    {
        public TestServiceProviderAttribute() : base(typeof(T))
        {
        }
    }
#else
    public sealed class TestServiceProviderAttribute : TestServiceProviderAttributeBase
    {
        public TestServiceProviderAttribute(Type serviceProviderType) : base(serviceProviderType)
        {

        }
    }
#endif

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public abstract class TestServiceProviderAttributeBase : Attribute
    {
        protected Type ServiceProviderType { get; }

        protected TestServiceProviderAttributeBase(Type serviceProviderType)
        {
        }
    }
}
