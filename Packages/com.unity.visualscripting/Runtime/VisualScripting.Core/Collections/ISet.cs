// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  ISet
**
** <OWNER>kimhamil</OWNER>
**
**
** Purpose: Base interface for all generic sets.
**
**
===========================================================*/

using System.Collections.Generic;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Generic collection that guarantees the uniqueness of its elements, as defined
    /// by a comparer. It also supports basic set operations such as Union, Intersection,
    /// Difference, and Symmetric Difference.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the set.</typeparam>
    public interface ISet<T> : ICollection<T>
    {
        /// <summary>
        /// Adds the specified item to the set.
        /// </summary>
        /// <param name="item">The item to add to the set.</param>
        /// <returns>
        /// <c>true</c> if the item was added to the set (i.e., it was not already present);
        /// otherwise, <c>false</c>.
        /// </returns>
        new bool Add(T item);

        /// <summary>
        /// Modifies the current set to contain all elements that are present in either the current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void UnionWith(IEnumerable<T> other);

        /// <summary>
        /// Modifies the current set to contain only elements that are present in both the current set and the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void IntersectWith(IEnumerable<T> other);

        /// <summary>
        /// Modifies the current set to contain only elements that are not present in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ExceptWith(IEnumerable<T> other);

        /// <summary>
        /// Modifies the current set to contain only elements that are present either in the current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void SymmetricExceptWith(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set is a subset of the specified collection; otherwise, <c>false</c>.
        /// </returns>
        bool IsSubsetOf(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set is a superset of the specified collection; otherwise, <c>false</c>.
        /// </returns>
        bool IsSupersetOf(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set is a proper subset of the specified collection. A proper subset is a subset that is not equal to the other set.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set is a proper subset of the specified collection; otherwise, <c>false</c>.
        /// </returns>
        bool IsProperSubsetOf(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set is a proper superset of the specified collection. A proper superset is a superset that is not equal to the other set.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set is a proper superset of the specified collection; otherwise, <c>false</c>.
        /// </returns>
        bool IsProperSupersetOf(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set has any elements in common with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set and the specified collection share at least one common element; otherwise, <c>false</c>.
        /// </returns>
        bool Overlaps(IEnumerable<T> other);

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// <c>true</c> if the current set and the specified collection contain the same elements; otherwise, <c>false</c>.
        /// </returns>
        bool SetEquals(IEnumerable<T> other);
    }
}
