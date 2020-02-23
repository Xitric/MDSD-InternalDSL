using System;
using InternalDSL.SemanticModel;

namespace InternalDSL.Builder
{
    /// <summary>
    /// Extensions methods for the IFluentComparisonBuilder when its output
    /// type is comparable, thus allowing for additional checks.
    /// </summary>
    public static class ComparisonBuilderExtensionMethods
    {
        /// <summary>
        /// Check if the output of the function under test is greater than the
        /// specified literal value.
        /// </summary>
        /// <param name="self">The progressive builder</param>
        /// <param name="literal">The literal value to compare against</param>
        /// <returns>The progressive builder</returns>
        public static IFluentComparisonCombinationBuilder<TInput, TOutput> IsGreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable
        {
            return self.AppendComparison(new LiteralComparison<TInput>(literal, EqualityOperator.GreaterThan));
        }

        /// <summary>
        /// Check if the output of the function under test is greater than the
        /// output of another function.
        /// </summary>
        /// <param name="self">The progressive builder</param>
        /// <param name="function">The function whose output to compare against</param>
        /// <returns>The progressive builder</returns>
        public static IFluentComparisonCombinationBuilder<TInput, TOutput> IsGreaterThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, dynamic> function)
            where TOutput : IComparable
        {
            return self.AppendComparison(new FunctionComparison<TInput>(function, EqualityOperator.GreaterThan));
        }

        /// <summary>
        /// Check if the output of the function under test is less than the
        /// specified literal value.
        /// </summary>
        /// <param name="self">The progressive builder</param>
        /// <param name="literal">The literal value to compare against</param>
        /// <returns>The progressive builder</returns>
        public static IFluentComparisonCombinationBuilder<TInput, TOutput> IsLessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, TOutput literal)
            where TOutput : IComparable
        {
            return self.AppendComparison(new LiteralComparison<TInput>(literal, EqualityOperator.LessThan));
        }

        /// <summary>
        /// Check if the output of the function under test is less than the
        /// output of another function.
        /// </summary>
        /// <param name="self">The progressive builder</param>
        /// <param name="function">The function whose output to compare against</param>
        /// <returns>The progressive builder</returns>
        public static IFluentComparisonCombinationBuilder<TInput, TOutput> IsLessThan<TInput, TOutput>(
            this IFluentComparisonBuilder<TInput, TOutput> self, Func<TInput, dynamic> function)
            where TOutput : IComparable
        {
            return self.AppendComparison(new FunctionComparison<TInput>(function, EqualityOperator.LessThan));
        }
    }
}
