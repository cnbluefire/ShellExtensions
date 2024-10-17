using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.Helpers
{
    internal class ServiceFactory : IServiceProvider
    {
        private ConcurrentDictionary<RuntimeTypeHandle, object> collection = new ConcurrentDictionary<RuntimeTypeHandle, object>();

        public object? GetService(Type serviceType)
        {
            if (collection.TryGetValue(serviceType.TypeHandle, out var obj))
            {
                return obj;
            }
            return null;
        }

        internal void UpdateFactory<T>(object? obj) => UpdateFactory(typeof(T).TypeHandle, obj);

        internal void UpdateFactory(RuntimeTypeHandle typeHandle, object? obj)
        {
            if (obj != null) collection[typeHandle] = obj;
            else collection.TryRemove(typeHandle, out _);
        }
    }

    public static class ServiceProviderExtensions
    {
        public static T? GetService<T>(this IServiceProvider provider) where T : class
        {
            return (T?)provider.GetService(typeof(T));
        }

        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            var obj = provider.GetService(serviceType);
            if (obj == null)
            {
                ThrowInvalidOperationException();
                return null!;
            }

            return obj;
        }

        public static T GetRequiredService<T>(this IServiceProvider provider) where T : class
        {
            return (T)GetRequiredService(provider, typeof(T));
        }

        private static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }
    }
}
