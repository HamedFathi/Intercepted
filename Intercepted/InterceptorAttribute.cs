using System;

namespace Castle.DynamicProxy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InterceptorAttribute : Attribute
    {
        public Type Interceptor { get; }
        public InterceptorAttribute(Type interceptor)
        {
            Interceptor = interceptor;
        }
    }
}
