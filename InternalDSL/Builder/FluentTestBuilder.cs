using System;
using System.Collections.Generic;
using System.Linq;
using InternalDSL.SemanticModel;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.Builder
{
    /// <summary>
    /// Interface for objects used to construct tests with a specific input
    /// type.
    /// </summary>
    public interface IFluentTestBuilder
    {
        IFluentTestBuilder Samples(int samples);

        IFluentTestBuilder<TInput> Generator<TInput>(Generator<TInput> generator);
    }

    /// <summary>
    /// Interface for objects used to construct tests with a variable number of
    /// properties.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    public interface IFluentTestBuilder<TInput>
    {
        IFluentPropertyBuilder<TInput> Property(string description);

        Test<TInput> Build();
    }

    /// <summary>
    /// Interface for objects used to configure a property with optional
    /// preconditions and a function under test.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    public interface IFluentPropertyBuilder<TInput>
    {
        IFluentPropertyBuilder<TInput> Given(Func<TInput, bool> function);

        IFluentComparisonBuilder<TInput, TOutput> Then<TOutput>(Func<TInput, TOutput> function);
    }

    /// <summary>
    /// Interface for objects used to specify the conditions that must be met
    /// for a property to be satisfied.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public interface IFluentComparisonBuilder<TInput, TOutput> : IFluentTestBuilder<TInput>
    {
        void AppendComparison(IComparison<TInput, TOutput> comparison);

        IFluentComparisonBuilder<TInput, TOutput> Equals(TOutput literal);
        IFluentComparisonBuilder<TInput, TOutput> Equals(Func<TInput, TOutput> function);

        IFluentComparisonBuilder<TInput, TOutput> IsNotEqual(TOutput literal);
        IFluentComparisonBuilder<TInput, TOutput> IsNotEqual(Func<TInput, TOutput> function);

        IFluentComparisonBuilder<TInput, TOutput> And();
        IFluentComparisonBuilder<TInput, TOutput> Or();
        IFluentComparisonBuilder<TInput, TOutput> BeginBlock();
        IFluentComparisonBuilder<TInput, TOutput> EndBlock();
    }

    public class FluentTestBuilder : IFluentTestBuilder
    {
        protected string Name { get; set; }
        protected int SampleCount { get; set; } = 100;

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

        public virtual Test<TInput> Build()
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
    }

    internal class FluentTestBuilder<TInput, TOutput> : FluentTestBuilder<TInput>, IFluentComparisonBuilder<TInput, TOutput>
    {
        internal Func<TInput, TOutput> Function;
        private readonly Stack<IComparison<TInput, TOutput>> _ongoingComparisons = new Stack<IComparison<TInput, TOutput>>();
        private readonly Stack<BooleanOperator> _ongoingOperators = new Stack<BooleanOperator>();
        private bool _shouldPush;
        private int _nestDepth;

        public void AppendComparison(IComparison<TInput, TOutput> comparison)
        {
            if (_shouldPush || !_ongoingComparisons.Any())
            {
                _ongoingComparisons.Push(comparison);
                _shouldPush = false;
            }
            else
            {
                var currentComparison = _ongoingComparisons.Pop();
                var op = _ongoingOperators.Pop(); //TODO: Ensure correct number of operators on stack
                var newComparison = new BlockComparison<TInput, TOutput>(currentComparison, comparison, op);
                _ongoingComparisons.Push(newComparison);
            }
        }

        public IFluentComparisonBuilder<TInput, TOutput> Equals(TOutput literal)
        {
            AppendComparison(new LiteralEqualityComparison<TInput, TOutput>(literal));
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> Equals(Func<TInput, TOutput> function)
        {
            AppendComparison(new FunctionEqualityComparison<TInput, TOutput>(function));
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> IsNotEqual(TOutput literal)
        {
            AppendComparison(new LiteralEqualityComparison<TInput, TOutput>(literal, false));
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> IsNotEqual(Func<TInput, TOutput> function)
        {
            AppendComparison(new FunctionEqualityComparison<TInput, TOutput>(function, false));
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> And()
        {
            _ongoingOperators.Push(BooleanOperator.AND);
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> Or()
        {
            _ongoingOperators.Push(BooleanOperator.OR);
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> BeginBlock()
        {
            _nestDepth++;
            _shouldPush = true;
            return this;
        }

        public IFluentComparisonBuilder<TInput, TOutput> EndBlock()
        {
            if (_ongoingComparisons.Count > _nestDepth)
            {
                var right = _ongoingComparisons.Pop();
                var left = _ongoingComparisons.Pop();
                var op = _ongoingOperators.Pop();
                //TODO: Something with ensuring both stacks have similar heights (operator stack 1 smaller)

                var newComparison = new BlockComparison<TInput, TOutput>(left, right, op);

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

        public override Test<TInput> Build()
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

            if (! _ongoingComparisons.Any())
            {
                throw new InvalidOperationException("Missing comparison for new property");
            }

            if (_ongoingComparisons.Count > 1)
            {
                throw new InvalidOperationException("Unfinished comparison for new property");
            }

            var property = new Property<TInput, TOutput>(Description, Function, _ongoingComparisons.Pop());
            SemanticModel.AddProperty(property);
        }
    }

    /// <summary>
    /// Extensions methods for the IFluentComparisonBuilder when its output
    /// type is comparable, thus allowing for additional checks.
    /// </summary>
    public static class ComparisonBuilderExtensionMethods
    {
        public static IFluentComparisonBuilder<TInput, TOutput> IsGreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable<TOutput>
        {
            self.AppendComparison(new LiteralComparison<TInput, TOutput>(literal, EqualityOperator.GreaterThan));
            return self;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> IsGreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, TOutput> function)
            where TOutput : IComparable<TOutput>
        {
            self.AppendComparison(new FunctionComparison<TInput, TOutput>(function, EqualityOperator.GreaterThan));
            return self;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> IsLessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable<TOutput>
        {
            self.AppendComparison(new LiteralComparison<TInput, TOutput>(literal, EqualityOperator.LessThan));
            return self;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> IsLessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, TOutput> function)
            where TOutput : IComparable<TOutput>
        {
            self.AppendComparison(new FunctionComparison<TInput, TOutput>(function, EqualityOperator.LessThan));
            return self;
        }
    }
}
