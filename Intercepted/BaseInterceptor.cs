using System;
using System.Linq;
using System.Reflection;

namespace Castle.DynamicProxy
{
    public class BaseInterceptor : IInterceptor
    {
        private object GetDefaultValue(Type t)
        {
            if (t.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            return null;
        }
        public void Intercept(IInvocation invocation)
        {
            try
            {
                OnEntry(invocation);
                invocation.Proceed();
                OnSuccess(invocation);
            }
            catch (Exception ex)
            {
                invocation.ReturnValue = GetDefaultValue(invocation.Method.ReturnType);
                OnException(invocation, ex);
            }
            finally
            {
                OnExit(invocation);
            }
        }
        protected virtual void OnEntry(IInvocation invocation)
        {
        }
        protected virtual void OnException(IInvocation invocation, Exception ex)
        {
        }
        protected virtual void OnExit(IInvocation invocation)
        {
        }
        protected virtual void OnSuccess(IInvocation invocation)
        {
        }
    }
}
