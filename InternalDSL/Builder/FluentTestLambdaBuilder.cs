using System;
using InternalDSL.SemanticModel;

namespace InternalDSL.Builder
{
    internal class FluentTestLambdaBuilder<TInput, TOutput> : FluentTestBuilder<TInput>,
       IFluentComparisonLambdaBuilder<TInput, TOutput> where TOutput : class
    {
        internal Func<TInput, TOutput> Function;

        public IFluentTestBuilder<TInput> Satisfies(Func<IFluentComparisonLambdaBuilder<TInput, TOutput>, IComparison> comparisonBuilder)
        {
            var comparison = comparisonBuilder(this);
            var property = new Property<TInput, TOutput>(Description, Preconditions, Function, comparison);
            SemanticModel.AddProperty(property);
            return this;
        }

        public IComparison Equals(TOutput literal) =>
            new LiteralEqualityComparison<TInput>(literal);

        public IComparison Equals(Func<TInput, TOutput> function) =>
            new FunctionEqualityComparison<TInput>(function);

        public IComparison IsNotEqual(TOutput literal) =>
            new LiteralEqualityComparison<TInput>(literal, false);

        public IComparison IsNotEqual(Func<TInput, TOutput> function) =>
            new FunctionEqualityComparison<TInput>(function, false);

        public IComparison And(
            Func<IFluentComparisonLambdaBuilder<TInput, TOutput>, IComparison> left,
            Func<IFluentComparisonLambdaBuilder<TInput, TOutput>, IComparison> right)
        {
            var leftComparison = left(this);
            var rightComparison = right(this);

            return new BlockComparison(leftComparison, rightComparison, BooleanOperator.And);
        }

        public IComparison Or(
            Func<IFluentComparisonLambdaBuilder<TInput, TOutput>, IComparison> left,
            Func<IFluentComparisonLambdaBuilder<TInput, TOutput>, IComparison> right)
        {
            var leftComparison = left(this);
            var rightComparison = right(this);

            return new BlockComparison(leftComparison, rightComparison, BooleanOperator.Or);
        }
    }
}
