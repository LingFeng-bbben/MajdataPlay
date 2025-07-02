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
        public static void Throw<TException>() where TException : Exception , new()
        {
            throw new TException();
        }
        [DoesNotReturn]
        public static void Throw<TException>(TException e) where TException : Exception
        {
            throw e;
        }
        [DoesNotReturn]
        public static TResult Throw<TException, TResult>() where TException : Exception, new()
        {
            throw new TException();
        }
        [DoesNotReturn]
        public static TResult Throw<TException, TResult>(TException e) where TException : Exception
        {
            throw e;
        }
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
        public static void OutOfRange()
        {
            throw new ArgumentOutOfRangeException();
        }
        [DoesNotReturn]
        public static void OutOfRange(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
        [DoesNotReturn]
        public static void OutOfRange(string paramName,string msg)
        {
            throw new ArgumentOutOfRangeException(paramName, msg);
        }
        public static class If
        {
            public static void Throw<TException>([DoesNotReturnIf(true)] bool condition) where TException : Exception, new()
            {
                if(condition)
                {
                    throw new TException();
                }
            }
            public static void Throw<TException>(TException e, [DoesNotReturnIf(true)] bool condition) where TException : Exception
            {
                if (condition)
                {
                    throw e;
                }
            }
            public static TResult Throw<TException, TResult>([DoesNotReturnIf(true)] bool condition) where TException : Exception, new()
            {
                if (condition)
                {
                    throw new TException();
                }
                return default;
            }
            public static TResult Throw<TException, TResult>(TException e, [DoesNotReturnIf(true)] bool condition) where TException : Exception
            {
                if (condition)
                {
                    throw e;
                }
                return default;
            }
        }
    }
}
