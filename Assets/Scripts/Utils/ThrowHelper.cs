using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    static class ThrowHelper
    {
        [DoesNotReturn]
        public static void NotSupported()
        {
            throw new NotSupportedException();
        }
        [DoesNotReturn]
        public static void NotSupported(string message)
        {
            throw new NotSupportedException(message);
        }
        [DoesNotReturn]
        public static void NotImplemented()
        {
            throw new NotImplementedException();
        }
        [DoesNotReturn]
        public static void NotImplemented(string message)
        {
            throw new NotImplementedException(message);
        }
        [DoesNotReturn]
        public static void Throw<TException>(TException e) where TException : Exception
        {
            throw e;
        }
        
        public static void ThrowIf<TException>(TException e, [DoesNotReturnIf(true)] bool condition) where TException : Exception
        {
            if(condition)
            {
                throw e;
            }
        }
        [DoesNotReturn]
        public static TReturn NotSupported<TReturn>()
        {
            throw new NotSupportedException();
        }
        [DoesNotReturn]
        public static TReturn NotSupported<TReturn>(string message)
        {
            throw new NotSupportedException(message);
        }
        [DoesNotReturn]
        public static TReturn NotImplemented<TReturn>()
        {
            throw new NotImplementedException();
        }
        [DoesNotReturn]
        public static TReturn NotImplemented<TReturn>(string message)
        {
            throw new NotImplementedException(message);
        }
        public static TResult Throw<TExceprion, TResult>(TExceprion e) where TExceprion : Exception
        {
            throw e;
        }
        public static TResult ThrowIf<TException, TResult>(TException e, TResult value, [DoesNotReturnIf(true)] bool condition) where TException : Exception
        {
            if (condition)
            {
                throw e;
            }
            return value;
        }
    }
}
