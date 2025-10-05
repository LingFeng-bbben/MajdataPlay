using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ManagedBass;

/// <summary>
/// A Bass Marshaling class, used to add multi-framework support for common marshalling of types.
/// </summary>
public static class BassMarshal
{
    /// <summary>Marshals data from an unmanaged block of memory to a newly allocated managed object of the specified type.</summary>
    /// <param name="ptr">A pointer to an unmanaged block of memory.</param>
    /// <returns>A managed object containing the data pointed to by the <paramref name="ptr" /> parameter.</returns>
    /// <exception cref="T:System.ArgumentException">The <paramref name="structureType" /> parameter layout is not sequential or explicit.
    /// -or-
    /// The <paramref name="structureType" /> parameter is a generic type definition.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="structureType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.MissingMethodException">The class specified by <paramref name="structureType" /> does not have an accessible default constructor.</exception>
    public static T PtrToStructure<T>(IntPtr ptr)
    {
#if NETFRAMEWORK
        return (T)Marshal.PtrToStructure(ptr, typeof(T));
#else
        return Marshal.PtrToStructure<T>(ptr);
#endif
    }

    /// <summary>Returns the size of an unmanaged type in bytes.</summary>
    /// <param name="t">The type whose size is to be returned.</param>
    /// <returns>The size of the specified type in unmanaged code.</returns>
    /// <exception cref="T:System.ArgumentException">The <paramref name="t" /> parameter is a generic type definition.</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="t" /> parameter is <see langword="null" />.</exception>
    public static int SizeOf<T>() =>
#if NETFRAMEWORK
        Marshal.SizeOf(typeof(T));
#else
        Marshal.SizeOf<T>();
#endif

    /// <summary>Returns the size of an unmanaged type in bytes.</summary>
    /// <param name="obj">The type whose size is to be returned.</param>
    /// <returns>The size of the specified type in unmanaged code.</returns>
    /// <exception cref="T:System.ArgumentException">The <paramref name="t" /> parameter is a generic type definition.</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="t" /> parameter is <see langword="null" />.</exception>
    public static int SizeOf(object obj) => Marshal.SizeOf(obj);
}