using Gandalf.Core.Attributes;

namespace Gandalf.Tests;

// public class DependencyTests
// {
//     [Test]
//     [Injector<TestInjector>]
//     public Task ServiceProviderAttribute_Should_Initialize_Correctly([Inject] TestClass testClass)
//     {
//         if (testClass == null)
//             throw new Exception("TestClass instance is null");

//         return Task.CompletedTask;
//     }

//     public class TestClass
//     {
//     }
// }

// public interface IInjector
// {
//     T GetInstance<T>();
// }

// public class TestInjector : IInjector
// {
//     public T GetInstance<T>()
//     {
//         return Activator.CreateInstance<T>();
//     }
// }

// [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
// public class InjectorAttribute<T> : Attribute where T : class, IInjector
// {
//     public T CreateInstance()
//     {
//         // Logic to create and return an instance of T
//         return Activator.CreateInstance<T>();
//     }
// }
