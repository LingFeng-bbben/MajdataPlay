using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Buffers
{
    public interface IObjectPool<TElement>
    {
        int Capacity { get; set; }
        /// <summary>
        /// If the pool is static, it will not be dynamically expanded when there are no available elements.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Get an available element from the pool
        /// <para>
        /// For a non-expandable static pool, null is returned when there are no available elements in the pool.
        /// </para>
        /// </summary>
        /// <returns>a element in the pool</returns>
        TElement? Dequeue();
        /// <summary>
        /// Returns the used element to the pool
        /// </summary>
        /// <param name="element">The element obtained from the pool</param>
        /// <exception cref="ElementNotMatchException{TElement}">Thrown when the returned element is not an element in the pool.</exception>
        void Collect(in TElement element);
    }
}
