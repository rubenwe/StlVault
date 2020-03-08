using System;

namespace StlVault.Util.Commands 
{
    internal static class FuncExtensions
    {
        public static Func<object, bool> Wrap(this Func<bool> parameterLessFunc)
        {
            if (parameterLessFunc == null) return null;

            return _ => parameterLessFunc();
        }
    }
}