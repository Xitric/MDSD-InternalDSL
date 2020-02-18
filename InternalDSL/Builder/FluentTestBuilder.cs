using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using InternalDSL.SemanticModel;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.Builder
{
    //TODO: Use subtypes of this class to limit the methods than can be called
    //TODO: Each subtype should have the creator injected so that it can be returned
    //TODO: Use the function syntax for creating boolean conditions of comparison objects, use varargs to combine arbitrary amounts

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

        IFluentPropertyComparisonBuilder<TInput, TOutput> Then<TOutput>(Func<TInput, TOutput> function);
    }

    /// <summary>
    /// Interface for objects used to specify the conditions that must be met
    /// for a property to be satisfied.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public interface IFluentPropertyComparisonBuilder<TInput, TOutput> : IFluentTestBuilder<TInput>
    {
        IFluentTestBuilder<TInput> Satisfies(
            Func<IFluentComparisonBuilder<TInput, TOutput>, IFluentComparisonBuilder<TInput, TOutput>> builder);
    }

    /// <summary>
    /// Interface for objects used to construct compound boolean comparisons.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public interface IFluentComparisonBuilder<TInput, TOutput>
    {
        IComparison<TInput, TOutput> Comparison { get; }

        IFluentComparisonBuilder<TInput, TOutput> EqualTo(TOutput literal);
        IFluentComparisonBuilder<TInput, TOutput> EqualTo(Func<TInput, TOutput> function);

        IFluentComparisonBuilder<TInput, TOutput> DifferentFrom(TOutput literal);
        IFluentComparisonBuilder<TInput, TOutput> DifferentFrom(Func<TInput, TOutput> function);

        IFluentComparisonBuilder<TInput, TOutput> And(IFluentComparisonBuilder<TInput, TOutput> left,
            IFluentComparisonBuilder<TInput, TOutput> right);

        IFluentComparisonBuilder<TInput, TOutput> Or(IFluentComparisonBuilder<TInput, TOutput> left,
            IFluentComparisonBuilder<TInput, TOutput> right);

        IComparison<TInput, TOutput> Build();
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

        public IFluentPropertyComparisonBuilder<TInput, TOutput> Then<TOutput>(Func<TInput, TOutput> function)
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

    internal class FluentTestBuilder<TInput, TOutput> : FluentTestBuilder<TInput>,
        IFluentPropertyComparisonBuilder<TInput, TOutput>
    {
        internal Func<TInput, TOutput> Function;
        internal IComparison<TInput, TOutput> Comparison;

        public IFluentTestBuilder<TInput> Satisfies(
            Func<IFluentComparisonBuilder<TInput, TOutput>, IFluentComparisonBuilder<TInput, TOutput>> builder)
        {
            var comparison = builder(new FluentComparisonBuilder<TInput, TOutput>()).Build();

            if (comparison == null)
            {
                throw new ArgumentException("Property cannot satisfy null comparison", nameof(comparison));
            }

            var compoundComparison = new BlockComparison<TInput, TOutput>(Comparison, comparison, BooleanOperator.AND);
            Comparison = compoundComparison;

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

            if (Comparison == null)
            {
                throw new InvalidOperationException("Missing comparison for new property");
            }

            var prop = new Property<TInput, TOutput>(Description, Function, Comparison);
            SemanticModel.AddProperty(prop);
        }
    }

    internal class FluentComparisonBuilder<TInput, TOutput> : IFluentComparisonBuilder<TInput, TOutput>
    {
        public IComparison<TInput, TOutput> Comparison { get; private set; }

        public IFluentComparisonBuilder<TInput, TOutput> EqualTo(TOutput literal)
        {
            throw new NotImplementedException();
        }

        public IFluentComparisonBuilder<TInput, TOutput> EqualTo(Func<TInput, TOutput> function)
        {
            throw new NotImplementedException();
        }

        public IFluentComparisonBuilder<TInput, TOutput> DifferentFrom(TOutput literal)
        {
            throw new NotImplementedException();
        }

        public IFluentComparisonBuilder<TInput, TOutput> DifferentFrom(Func<TInput, TOutput> function)
        {
            throw new NotImplementedException();
        }

        public IFluentComparisonBuilder<TInput, TOutput> And(IFluentComparisonBuilder<TInput, TOutput> left, IFluentComparisonBuilder<TInput, TOutput> right)
        {
            throw new NotImplementedException();
        }

        public IFluentComparisonBuilder<TInput, TOutput> Or(IFluentComparisonBuilder<TInput, TOutput> left, IFluentComparisonBuilder<TInput, TOutput> right)
        {
            throw new NotImplementedException();
        }

        public IComparison<TInput, TOutput> Build()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Extensions methods for the IFluentComparisonBuilder when its output
    /// type is comparable, thus allowing for additional checks.
    /// </summary>
    public static class ComparisonBuilderExtensionMethods
    {
        public static IFluentComparisonBuilder<TInput, TOutput> GreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable<TOutput>
        {
            return null;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> GreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, TOutput> function)
            where TOutput : IComparable<TOutput>
        {
            return null;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> LessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable<TOutput>
        {
            return null;
        }

        public static IFluentComparisonBuilder<TInput, TOutput> LessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, TOutput> function)
            where TOutput : IComparable<TOutput>
        {
            return null;
        }
    }
}
