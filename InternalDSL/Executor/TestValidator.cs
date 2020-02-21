using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalDSL.SemanticModel;

namespace InternalDSL.Executor
{
    /// <summary>
    /// Utility class for easily creating a new TestValidator.
    /// </summary>
    public class TestValidator
    {
        public static TestValidator<TInput> Create<TInput>(ITest<TInput> a, ITest<TInput> b) =>
            new TestValidator<TInput>(a, b);
    }

    /// <summary>
    /// Class used to compare Test objects from the semantic model for
    /// equality.
    /// </summary>
    public class TestValidator<TInput>
    {
        public ITest<TInput> TestA { get; }
        public ITest<TInput> TestB { get; }

        public TestValidator(ITest<TInput> a, ITest<TInput> b)
        {
            TestA = a;
            TestB = b;
        }

        /// <summary>
        /// Execute this comparison as a unit test. If any difference between
        /// the test objects is found, the unit test will fail.
        /// </summary>
        public void Assert()
        {
            if (TestA.Name != TestB.Name)
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Test names do not match");

            if (TestA.Samples != TestB.Samples)
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Tests have different sample counts");

            if (TestA.Generator.GetType() != TestB.Generator.GetType())
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Tests have different generators");

            if (TestA.Properties.Count != TestB.Properties.Count)
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Tests have different numbers of properties");

            for (var i = 0; i < TestA.Properties.Count; i++)
            {
                var propertyA = TestA.Properties[i];
                var propertyB = TestB.Properties[i];
                AssertEquals(propertyA, propertyB);
            }
        }

        private void AssertEquals<TOutput>(IProperty<TInput, TOutput> a, IProperty<TInput, TOutput> b)
        {
            if (a.Description != b.Description)
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Properties have different descriptions");

            if (a.Function.GetType() != b.Function.GetType())
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Properties test different functions");

            var comparisonA = a.Comparison;
            var comparisonB = b.Comparison;
            AssertEquals(comparisonA, comparisonB);
        }

        private void AssertEquals(IComparison a, IComparison b)
        {
            if (a.GetType() != b.GetType())
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Comparison of type {a.GetType()} is not equal to {b.GetType()}");

            //We know that the comparisons have equal type
            switch (a)
            {
                case BlockComparison typedA1:
                    var typedB1 = (BlockComparison)b;
                    AssertEquals(typedA1.Left, typedB1.Left);
                    AssertEquals(typedA1.Right, typedB1.Right);
                    break;
                case FunctionComparison<TInput> typedA2:
                    var typedB2 = (FunctionComparison<TInput>)b;
                    if (typedA2.CheckEqual != typedB2.CheckEqual ||
                        typedA2.EqualityOperator != typedB2.EqualityOperator ||
                        !typedA2.ExpectedFunction.Equals(typedB2.ExpectedFunction))
                        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Mismatching function comparisons detected");
                    break;
                case FunctionEqualityComparison<TInput> typedA3:
                    var typedB3 = (FunctionEqualityComparison<TInput>)b;
                    if (typedA3.CheckEqual != typedB3.CheckEqual ||
                        typedA3.ExpectedFunction.GetType() != typedB3.ExpectedFunction.GetType())
                        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Mismatching function equality comparisons detected");
                    break;
            }
        }
    }
}
