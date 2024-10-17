using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.Helpers
{
    internal class FastActivator
    {
        public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>()
            where T : new()
        {
            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                return FastActivatorImpl<T>.NewFunction();
            }
            else
            {
                return new T();
            }
        }

        private class FastActivatorImpl<T> where T : new()
        {
            private static readonly Expression<Func<T>> NewExpression = () => new T();
            internal static readonly Func<T> NewFunction = NewExpression.Compile();
        }
    }
}
