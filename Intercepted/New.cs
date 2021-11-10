using System;
using System.Linq;
using System.Reflection;

namespace Castle.DynamicProxy
{
    public static class New
    {
        public static TClass Of<TClass>()
            where TClass : class, new()
        {
            var interceptors = typeof(TClass).GetTypeInfo().GetCustomAttributes(typeof(InterceptorAttribute), false)
                .Cast<InterceptorAttribute>()
                .Select(x => x.Interceptor)
                .ToList();

            var instances = interceptors.Select(x => Activator.CreateInstance(x)).Cast<IInterceptor>().ToArray();

            var generator = new ProxyGenerator();
            return generator.CreateClassProxy<TClass>(instances);
        }
        public static TClass Of<TClass>(params IInterceptor[] interceptors)
            where TClass : class, new()
        {
            var interceptors1 = typeof(TClass).GetTypeInfo().GetCustomAttributes(typeof(InterceptorAttribute), false)
                .Cast<InterceptorAttribute>()
                .Select(x => x.Interceptor)
                .ToList();

            var instances = interceptors1.Select(x => Activator.CreateInstance(x)).Cast<IInterceptor>().ToList();

            if (interceptors != null && interceptors.Length > 0)
            {
                instances.AddRange(interceptors);
            }

            var generator = new ProxyGenerator();
            return generator.CreateClassProxy<TClass>(instances.ToArray());
        }
        public static TClass Of<TClass>(ProxyGenerationOptions options, params IInterceptor[] interceptors)
                            where TClass : class, new()
        {
            var generator = new ProxyGenerator();
            return generator.CreateClassProxy<TClass>(options, interceptors);
        }
    }
}
