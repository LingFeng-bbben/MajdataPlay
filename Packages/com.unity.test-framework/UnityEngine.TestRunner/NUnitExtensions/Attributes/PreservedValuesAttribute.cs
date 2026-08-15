using NUnit.Framework;
using UnityEngine.Scripting;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Like the ValuesAttribute it is used to provide literal arguments for
    /// an individual parameter of a test. It has the Preserve attribute added,
    /// allowing it to persist in players build at a high stripping level.
    /// </summary>
    [Preserve]
    public class PreservedValuesAttribute : ValuesAttribute
    {
        /// <summary>
        /// Construct the values attribute with a set of arguments.
        /// </summary>
        /// <param name="args">The set of arguments for the test parameter.</param>
        public PreservedValuesAttribute(params object[] args) : base(args)
        {

        }
    }
}
