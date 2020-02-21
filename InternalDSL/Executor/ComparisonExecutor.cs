using System;
using InternalDSL.SemanticModel;

namespace InternalDSL.Executor
{
    internal class ComparisonExecutor<TInput>
    {
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
