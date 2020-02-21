﻿using System;
using InternalDSL.SemanticModel;

namespace InternalDSL.Executor
{
    /// <summary>
    /// Class used to execute boolean comparisons and throw descriptive
    /// exceptions when a violation has been detected. This exception is used
    /// to fail the unit test.
    ///
    /// I would have liked to make this more dynamic and less hard-coded, but I
    /// struggled enough already with simply making the generics work.
    /// </summary>
    internal class ComparisonExecutor<TInput>
    {
        /// <summary>
        /// Assert that given the input sample, the boolean expression will be
        /// true for all comparisons with the functionValue.
        /// </summary>
        /// <param name="sample">The input sample used to generate the functionValue</param>
        /// <param name="functionValue">The value generated by the function under test</param>
        /// <param name="comparison">The boolean expression to evaluate the functionValue against</param>
        internal void Assert<TOutput>(TInput sample, TOutput functionValue, IComparison comparison)
        {
            switch (comparison)
            {
                case BlockComparison blockComparison:
                    AssertBlock(sample, functionValue, blockComparison);
                    break;
                case FunctionComparison<TInput> functionComparison:
                    if (!(functionValue is IComparable<TOutput>))
                        throw new PropertyException("Attempted to compare output values that are not of type IComparable");

                    AssertFunction(sample, (IComparable<TOutput>)functionValue, functionComparison);
                    break;
                case FunctionEqualityComparison<TInput> functionEqualityComparison:
                    AssertFunctionEquality(sample, functionValue, functionEqualityComparison);
                    break;
                default:
                    throw new PropertyException($"Unexpected comparison type {comparison.GetType().Name}");
            }
        }

        /// <summary>
        /// Utility method to capture the exception thrown by executing a
        /// boolean expression.
        /// </summary>
        private PropertyException AssertForException<TOutput>(TInput sample, TOutput functionValue, IComparison comparison)
        {
            try
            {
                Assert(sample, functionValue, comparison);
            }
            catch (PropertyException ex)
            {
                return ex;
            }

            return null;
        }

        /// <summary>
        /// Evaluate a compound boolean expression by evaluating each child
        /// expression in turn and combining them with the appropriate boolean
        /// operator.
        /// </summary>
        private void AssertBlock<TOutput>(TInput sample, TOutput functionValue,
            BlockComparison comparison)
        {
            var leftException = AssertForException(sample, functionValue, comparison.Left);
            var rightException = AssertForException(sample, functionValue, comparison.Right);

            switch (comparison.Operator)
            {
                case BooleanOperator.And:
                    if (leftException != null || rightException != null)
                        throw leftException ?? rightException;
                    break;
                case BooleanOperator.Or:
                    if (leftException != null && rightException != null)
                        throw new PropertyException($"{leftException.Message}\n{rightException.Message}");
                    break;
                default:
                    throw new PropertyException($"Unexpected boolean operator {comparison.Operator}");
            }
        }

        /// <summary>
        /// Evaluate a boolean comparison between two function values that
        /// implement IComparable.
        /// </summary>
        private void AssertFunction<TOutput>(TInput sample, IComparable<TOutput> functionValue,
            FunctionComparison<TInput> comparison)
        {
            var expected = (TOutput) comparison.ExpectedFunction(sample);
            if (functionValue != null)
                if (Math.Sign(functionValue.CompareTo(expected)) != (int) comparison.EqualityOperator)
                    throw new PropertyException($"Expected {functionValue} to relate to {expected} by operator {comparison.EqualityOperator}");

            switch (comparison.EqualityOperator)
            {
                case EqualityOperator.EqualTo:
                    if (expected != null)
                        throw new PropertyException($"Expected null to be equal to {expected}");
                    break;
                case EqualityOperator.LessThan:
                    if (expected == null)
                        throw new PropertyException($"Expected null to be less than null");
                    break;
                case EqualityOperator.GreaterThan:
                    throw new PropertyException($"Expected null to be greater than {expected}");
                default:
                    throw new PropertyException($"Unexpected equality operator {comparison.EqualityOperator}");
            }
        }

        /// <summary>
        /// Evaluate the equality or inequality of two function values.
        /// </summary>
        private void AssertFunctionEquality<TOutput>(TInput sample, TOutput functionValue,
            FunctionEqualityComparison<TInput> comparison)
        {
            var expected = comparison.ExpectedFunction(sample);
            if (expected == null)
            {
                if ((functionValue == null) != comparison.CheckEqual)
                    throw new PropertyException($"{functionValue} is not {(comparison.CheckEqual ? "equal to" : "different from")} null");
            }
            else if (expected.Equals(functionValue) != comparison.CheckEqual)
                throw new PropertyException($"{functionValue} is not {(comparison.CheckEqual ? "equal to" : "different from")} {expected}");
        }
    }
}
