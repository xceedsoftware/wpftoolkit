using System;
using Microsoft.Practices.Unity;

namespace Samples.Infrastructure.Extensions
{
    public static class UnityContainerExtensions
    {
        public static void RegisterNavigationType(this IUnityContainer container, Type type)
        {
            container.RegisterType(typeof(Object), type, type.FullName);
        }
    }
}
