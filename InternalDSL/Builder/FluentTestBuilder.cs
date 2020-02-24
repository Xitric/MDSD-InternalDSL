using System;
using System.Collections.Generic;
using System.Linq;
using InternalDSL.SemanticModel;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.Builder
{
    public class FluentTestBuilder : IFluentTestBuilder
    {
        protected string Name { get; set; }
        protected int SampleCount { get; set; } = 100;

        /// <summary>
        /// Begin the chain for building a new test. Usually this method will
        /// be available through static importing.
        /// </summary>
        /// <param name="name">The name of the test, used for referring to it and for printing if the test fails</param>
        /// <returns>The progressive builder</returns>
        public static FluentTestBuilder Test(string name)
        {
            return new FluentTestBuilder() { Name = name };
        }

        public IFluentTestBuilder Samples(int samples)
        {
            SampleCount = samples;
            return this;
        }

        public IFluentTestBuilder<TInput> Generator<TInput>(Generator<TInput> generator)
        {
            return new FluentTestBuilder<TInput>()
            {
                Name = Name,
                SampleCount = SampleCount,
                Generator = generator,
                SemanticModel = new Test<TInput>(Name, SampleCount, generator)
            };
        }
    }

    internal class FluentTestBuilder<TInput> : FluentTestBuilder, IFluentTestBuilder<TInput>, IFluentPropertyBuilder<TInput>
    {
        internal Generator<TInput> Generator { get; set; }
        protected string Description;
        protected IList<Func<TInput, bool>> Preconditions;
        internal Test<TInput> SemanticModel { get; set; }

        public virtual IFluentPropertyBuilder<TInput> Property(string description)
        {
            Preconditions = null;
            Description = description;
            return this;
        }

        public virtual ITest<TInput> Build()
        {
            return SemanticModel;
        }

        public IFluentPropertyBuilder<TInput> Given(Func<TInput, bool> function)
        {
            if (Preconditions == null)
            {
                Preconditions = new List<Func<TInput, bool>>();
            }

            Preconditions.Add(function);
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> Then<TOutput>(Func<TInput, TOutput> function)
            where TOutput : class
        {
            //"Upgrade" to a builder that now also knows the output type
            return new FluentTestBuilder<TInput, TOutput>()
            {
                Name = Name,
                SampleCount = SampleCount,
                Generator = Generator,
                SemanticModel = SemanticModel,
                Description = Description,
                Preconditions = Preconditions,
                Function = function
            };
        }

        public IFluentComparisonLambdaBuilder<TInput, TOutput> ThenLambda<TOutput>(Func<TInput, TOutput> function)
            where TOutput : class
        {
            //"Upgrade" to a builder that now also knows the output type and uses lambdas
            return new FluentTestLambdaBuilder<TInput, TOutput>()
            {
                Name = Name,
                SampleCount = SampleCount,
                Generator = Generator,
                SemanticModel = SemanticModel,
                Description = Description,
                Preconditions = Preconditions,
                Function = function
            };
        }
    }

    internal class FluentTestBuilder<TInput, TOutput> : FluentTestBuilder<TInput>,
        IFluentComparisonBuilder<TInput, TOutput>, IFluentComparisonCombinationBuilder<TInput, TOutput> where TOutput : class
    {
        internal Func<TInput, TOutput> Function;
        private readonly Stack<IComparison> _ongoingComparisons = new Stack<IComparison>();
        private readonly Stack<BooleanOperator> _ongoingOperators = new Stack<BooleanOperator>();
        private bool _shouldPush;
        private int _nestDepth;

        public IFluentComparisonCombinationBuilder<TInput, TOutput> AppendComparison(
            IComparison comparison)
        {
            if (_shouldPush || !_ongoingComparisons.Any())
            {
                _ongoingComparisons.Push(comparison);
                _shouldPush = false;
            }
            else
            {
                if (_ongoingComparisons.Count != _ongoingOperators.Count)
                    throw new InvalidOperationException("Mismatching operators and boolean comparisons");

                var currentComparison = _ongoingComparisons.Pop();
                var op = _ongoingOperators.Pop();
                var newComparison = new BlockComparison(currentComparison, comparison, op);
                _ongoingComparisons.Push(newComparison);
            }

            return this;
        }

        public IFluentComparisonCombinationBuilder<TInput, TOutput> Equals(TOutput literal) =>
            AppendComparison(new LiteralEqualityComparison<TInput>(literal));

        public IFluentComparisonCombinationBuilder<TInput, TOutput> Equals(Func<TInput, TOutput> function) =>
            AppendComparison(new FunctionEqualityComparison<TInput>(function));

        public IFluentComparisonCombinationBuilder<TInput, TOutput> IsNotEqual(TOutput literal) =>
            AppendComparison(new LiteralEqualityComparison<TInput>(literal, false));

        public IFluentComparisonCombinationBuilder<TInput, TOutput> IsNotEqual(Func<TInput, TOutput> function) =>
            AppendComparison(new FunctionEqualityComparison<TInput>(function, false));

        public IFluentComparisonBuilder<TInput, TOutput> And()
        {
            _ongoingOperators.Push(BooleanOperator.And);
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> Or()
        {
            _ongoingOperators.Push(BooleanOperator.Or);
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> BeginBlock()
        {
            _nestDepth++;
            _shouldPush = true;
            return this;
        }

        public IFluentComparisonCombinationBuilder<TInput, TOutput> EndBlock()
        {
            if (_ongoingComparisons.Count > _nestDepth)
            {
                if (_ongoingComparisons.Count != _ongoingOperators.Count + 1)
                    throw new InvalidOperationException("Mismatching operators and boolean comparisons");

                var right = _ongoingComparisons.Pop();
                var left = _ongoingComparisons.Pop();
                var op = _ongoingOperators.Pop();

                var newComparison = new BlockComparison(left, right, op);

                _ongoingComparisons.Push(newComparison);
            }

            _nestDepth--;
            return this;
        }

        public override IFluentPropertyBuilder<TInput> Property(string description)
        {
            FlushProperty();
            return base.Property(description);
        }

        public override ITest<TInput> Build()
        {
            FlushProperty();
            return base.Build();
        }

        private void FlushProperty()
        {
            if (Description == null)
            {
                throw new InvalidOperationException("Missing description for new property");
            }

            if (Function == null)
            {
                throw new InvalidOperationException("Missing test function for new property");
            }

            if (!_ongoingComparisons.Any())
            {
                throw new InvalidOperationException("Missing comparison for new property");
            }

            if (_ongoingComparisons.Count > 1 || _ongoingOperators.Any())
            {
                throw new InvalidOperationException("Unfinished comparison for new property");
            }

            var property = new Property<TInput, TOutput>(Description, Preconditions, Function, _ongoingComparisons.Pop());
            SemanticModel.AddProperty(property);
        }
    }
}
