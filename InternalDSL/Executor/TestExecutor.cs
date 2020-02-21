using InternalDSL.SemanticModel;

namespace InternalDSL.Executor
{
    public class TestExecutor
    {
        public static TestExecutor<TInput> Create<TInput>(Test<TInput> test) =>
            new TestExecutor<TInput>(test);
    }

    public class TestExecutor<TInput>
    {
        public Test<TInput> Test { get; }
        private readonly ComparisonExecutor<TInput> _comparisonExecutor;

        public TestExecutor(Test<TInput> test)
        {
            Test = test;
            _comparisonExecutor = new ComparisonExecutor<TInput>();
        }

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
