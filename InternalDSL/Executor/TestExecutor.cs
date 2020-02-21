using InternalDSL.SemanticModel;

namespace InternalDSL.Executor
{
    /// <summary>
    /// Utility class for easily creating a new TestExecutor.
    /// </summary>
    public class TestExecutor
    {
        public static TestExecutor<TInput> Create<TInput>(ITest<TInput> test) =>
            new TestExecutor<TInput>(test);
    }

    /// <summary>
    /// Class used to execute Test objects from the semantic model. It will
    /// automatically sample the input generator and evaluate each property in
    /// turn.
    /// </summary>
    public class TestExecutor<TInput>
    {
        public ITest<TInput> Test { get; }
        private readonly ComparisonExecutor<TInput> _comparisonExecutor;

        public TestExecutor(ITest<TInput> test)
        {
            Test = test;
            _comparisonExecutor = new ComparisonExecutor<TInput>();
        }

        /// <summary>
        /// Execute this test as a unit test. If any violation is found within
        /// a property, the unit test will fail.
        /// </summary>
        public void Assert()
        {
            for (var i = 0; i < Test.Samples; i++)
            {
                var sample = Test.Generator.Next();
                foreach (var property in Test.Properties)
                {
                    Assert(property, sample);
                }
            }
        }

        private void Assert<TOutput>(IProperty<TInput, TOutput> property, TInput sample)
        {
            var functionValue = property.Function(sample);

            try
            {
                _comparisonExecutor.Assert(sample, functionValue, property.Comparison);
            }
            catch (PropertyException ex)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(
                    $"Failed \"{Test.Name}\"\n" +
                    $"Property:\n" +
                    $"\t{property.Description}\n" +
                    $"Input sample:\n" +
                    $"\t{sample}\n" +
                    $"{ex.Message}");
            }
        }
    }
}
