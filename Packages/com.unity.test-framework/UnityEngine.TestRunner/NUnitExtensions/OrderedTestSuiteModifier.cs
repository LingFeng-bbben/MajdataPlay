

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestRunner.NUnitExtensions
{
    internal class OrderedTestSuiteModifier : ITestSuiteModifier
    {
        internal const string suiteIsReorderedProperty = "suiteIsReordered";
        private string[] m_OrderedTestNames;
        private readonly int m_randomOrderSeed;

        public OrderedTestSuiteModifier(string[] orderedTestNames, int randomOrderSeed)
        {
            m_OrderedTestNames = orderedTestNames;
            m_randomOrderSeed = randomOrderSeed;
        }

        public TestSuite ModifySuite(TestSuite root)
        {
            if((m_OrderedTestNames == null || m_OrderedTestNames?.Length == 0) && m_randomOrderSeed == 0)
            {
                return root;
            }
            // If we don't have a orderList but we do have a random seed, we need to generate a random order list
            if ((m_OrderedTestNames == null || m_OrderedTestNames.Length == 0) && m_randomOrderSeed != 0)
            {
                var testlist = GetAllTestList(root);
                var rand = new System.Random(m_randomOrderSeed);
                var randomNumberFromSeed = rand.Next();
                var shuffledList = testlist.OrderBy(fullName => GetHash(fullName, randomNumberFromSeed)).ToList();

                m_OrderedTestNames = shuffledList.ToArray();
            }

            var suite = new TestSuite(root.Name);
            suite.Properties.Set(suiteIsReorderedProperty, true);
            var workingStack = new List<ITest> { suite };

            foreach (var fullName in m_OrderedTestNames)
            {
                var test = FindTest(root, fullName);
                if (test == null)
                {
                    continue;
                }

                workingStack = InsertTestInCurrentStackIncludingAllMissingAncestors(test, workingStack);
            }

            // When the testlist interleaves tests from the same fixture with tests from other fixtures, that fixture
            // (and any ancestor TestSuite/TestAssembly that diverges along the way) gets cloned multiple times. All
            // clones share the same Name and FullName, so Test.GetUniqueName() returns identical strings for every
            // clone. The default UnityWorkItemDataHolder.alreadyExecutedTests path then treats every clone after the
            // first as already executed once a domain reload happens mid-run: ShouldRestore returns true,
            // m_DontRunRestoringResult is set, and PerformOneTimeSetUp is skipped. The children then run with a null
            // fixture instance bound to the TestExecutionContext, surfacing as
            // "SetUp/TearDown : Non-static method requires a target."
            //
            // Disambiguate by stamping each cloned suite with a globally unique "childIndex" property so
            // GetUniqueName() yields a distinct string per clone. We use a single monotonically increasing counter
            // (rather than per-parent counts via ParseForNameDuplicates) because a fixture can also be cloned
            // indirectly through a cloned ancestor (e.g. a namespace TestSuite cloned twice each containing one
            // fixture clone). A per-parent counter would leave such fixture clones indistinguishable since their
            // GetAncestorPath uses the ancestor's Name (which is identical across clones), not its childIndex.
            AssignUniqueClonedSuiteIndexes(suite);

            return suite;
        }

        private static void AssignUniqueClonedSuiteIndexes(ITest root)
        {
            int counter = 0;
            AssignUniqueClonedSuiteIndexes(root, ref counter);
        }

        private static void AssignUniqueClonedSuiteIndexes(ITest test, ref int counter)
        {
            foreach (var child in test.Tests)
            {
                if (child.IsSuite && child.Properties.ContainsKey(suiteIsReorderedProperty))
                {
                    counter++;
                    // childIndex is consumed by TestExtensions.GetUniqueName when building the unique identifier
                    // for a test. A per-clone counter makes every cloned suite distinguishable, which prevents the
                    // alreadyExecutedTests/ShouldRestore short-circuit from skipping PerformOneTimeSetUp on later
                    // clones of the same fixture.
                    child.Properties.Set("childIndex", counter);
                }
                AssignUniqueClonedSuiteIndexes(child, ref counter);
            }
        }

        private static List<ITest> InsertTestInCurrentStackIncludingAllMissingAncestors(ITest test, List<ITest> newAncestorStack)
        {
            var originalAncestorStack = GetAncestorStack(test);

            // We can start looking at index 1 in the stack, as all elements are assumed to share the same top root.
            for (int i = 1; i < originalAncestorStack.Count; i++)
            {
                if (DoAncestorsDiverge(newAncestorStack, originalAncestorStack, i))
                {
                    // The ancestor list diverges from the current working stack so insert a new element
                    var commonParent = newAncestorStack[i - 1];
                    var nodeToClone = originalAncestorStack[i];

                    var newNode = CloneNode(nodeToClone);
                    (commonParent as TestSuite).Add(newNode);
                    if (i < newAncestorStack.Count)
                    {
                        // Remove the diverging element and all its children.
                        newAncestorStack = newAncestorStack.Take(i).ToList();
                    }

                    newAncestorStack.Add(newNode);
                }
            }

            return newAncestorStack;
        }

        private static bool DoAncestorsDiverge(List<ITest> newAncestorStack, List<ITest> originalAncestorStack, int i)
        {
            return i >= newAncestorStack.Count || originalAncestorStack[i].Name != newAncestorStack[i].Name || !originalAncestorStack[i].HasChildren;
        }

        private static Test CloneNode(ITest test)
        {
            var type = test.GetType();
            Test newTest;
            if (type == typeof(TestSuite))
            {
                newTest = new TestSuite(test.Name);
            }
            else if (type == typeof(TestAssembly))
            {
                var testAssembly = (TestAssembly)test;
                newTest = new TestAssembly(testAssembly.Assembly, testAssembly.Name);;
            }
            else if (type == typeof(TestFixture))
            {
                var existingFixture = (TestFixture)test;
                newTest = new TestFixture(test.TypeInfo);
                if (existingFixture.Arguments?.Length > 0)
                {
                    // Newer versions of NUnit has a constructor that allows for setting this argument. Our custom NUnit version only allows for setting it through reflection at the moment.
                    typeof(TestFixture).GetProperty(nameof(existingFixture.Arguments)).SetValue(newTest, existingFixture.Arguments);
                }
            }
            else if (type == typeof(TestMethod))
            {
                // On the testMethod level, it is safe to reuse the node.
                newTest = test as Test;
            }
            else if (type == typeof(ParameterizedMethodSuite))
            {
                newTest = new ParameterizedMethodSuite(test.Method);
            }
            else if (type == typeof(ParameterizedFixtureSuite))
            {
                newTest = new ParameterizedFixtureSuite(test.Tests[0].TypeInfo);
            }
            else if (type == typeof(SetUpFixture))
            {
                newTest = new SetUpFixture(test.TypeInfo);
            }
            else
            {
                // If there are any node types that we do not know how to handle, then we should fail hard, so they can be added.
                throw new NotImplementedException(type.FullName);
            }

            CloneProperties(newTest, test);
            newTest.RunState = test.RunState;
            newTest.Properties.Set(suiteIsReorderedProperty, true);
            return newTest;
        }

        private static void CloneProperties(ITest target, ITest source)
        {
            if (target == source)
            {
                // On the TestMethod level, the node is reused, so do not clone the node properties.
                return;
            }

            foreach (var key in source.Properties.Keys)
            {
                foreach (var value in source.Properties[key])
                {
                    target.Properties.Set(key, value);
                }
            }
        }

        private static List<ITest> GetAncestorStack(ITest test)
        {
            var list = new List<ITest>();
            while (test != null)
            {
                list.Insert(0, test);
                test = test.Parent;
            }

            return list;
        }

        private static List<string> GetAllTestList(ITest test)
        {
            var listOfTests = new List<string>();

            if (test.IsSuite)
            {
                listOfTests.AddRange(test.Tests.SelectMany(GetAllTestList));
            }
            else
            {
                listOfTests.Add(test.FullName);
            }
            return listOfTests;
        }

        private static int GetHash(string fullName, int randomNumber)
        {
            var hash = 0;
            foreach (var c in fullName)
            {
                hash = hash * 31 + c;
            }

            return hash ^ randomNumber;
        }

        private static ITest FindTest(ITest node, string fullName)
        {
            if (node.HasChildren)
            {
                return node.Tests
                    .Select(test => FindTest(test, fullName))
                    .FirstOrDefault(match => match != null);
            }

            return node.FullName == fullName ? node : null;
        }
    }
}
