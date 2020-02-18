﻿using System;

namespace InternalDSL.SemanticModel
{
    /// <summary>
    /// Used to configure the type of comparison to be performed.
    /// </summary>
    public enum EqualityOperator
    {
        LessThan = -1,
        EqualTo = 0,
        GreaterThan = 1
    }

    /// <summary>
    /// Used tp configure how predicates are combined into more complex predicates.
    /// </summary>
    public enum BooleanOperator
    {
        AND = 0,
        OR = 1
    }

    /// <summary>
    /// Instances of this class are used to assert expectations on return
    /// values from functions under test. In the context of propositions, a
    /// comparison can be thought of as the propositional function.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public interface IComparison<in TInput, in TOutput>
    {
        /// <summary>
        /// Asserts whether a function under test is expected to generate the
        /// specified function value when provided with the given input sample.
        /// For instance, the input (2, 3) to an add function might result in
        /// the functionValue 5. This value is then matched against the rules
        /// encapsulated in this comparison instance.
        /// </summary>
        /// <param name="inputSample">The input generated by the test for a particular sample</param>
        /// <param name="functionValue">The output generated by the function under test</param>
        /// <returns>True if the function value is within expectations, false otherwise</returns>
        bool Matches(TInput inputSample, TOutput functionValue);
    }

    /// <summary>
    /// Used to compare the output of the function under test with the output
    /// of another function for equality or inequality only.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public class FunctionEqualityComparison<TInput, TOutput> : IComparison<TInput, TOutput>
    {
        protected readonly Func<TInput, TOutput> expectedFunction;
        private readonly bool _equal;

        public FunctionEqualityComparison(Func<TInput, TOutput> expectedFunction, bool equal = true)
        {
            this.expectedFunction = expectedFunction;
            _equal = equal;
        }

        public virtual bool Matches(TInput inputSample, TOutput functionValue)
        {
            var expected = expectedFunction(inputSample);
            if (expected == null)
            {
                return (functionValue == null) == _equal;
            }

            return (expected.Equals(functionValue)) == _equal;
        }
    }

    /// <summary>
    /// Used to compare the output of the function under test with the output
    /// of another function. This class supports both equality, less than, and
    /// greater than checks.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public class FunctionComparison<TInput, TOutput> : FunctionEqualityComparison<TInput, TOutput> where TOutput : IComparable<TOutput>
    {
        private readonly EqualityOperator _equalityOperator;

        public FunctionComparison(Func<TInput, TOutput> expectedFunction, EqualityOperator equalityOperator) : base(expectedFunction)
        {
            _equalityOperator = equalityOperator;
        }

        public override bool Matches(TInput inputSample, TOutput functionValue)
        {
            var expected = expectedFunction(inputSample);
            if (functionValue != null)
            {
                return Math.Sign(functionValue.CompareTo(expected)) == (int)_equalityOperator;
            }

            switch (_equalityOperator)
            {
                case EqualityOperator.EqualTo:
                    return expected == null;
                case EqualityOperator.LessThan: //Null is only less than something that is not null
                    return expected != null;
                case EqualityOperator.GreaterThan: //Null cannot be greater than anything
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Used to compare a function output with an invariant value for equality
    /// or inequality only.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public class LiteralEqualityComparison<TInput, TOutput> : FunctionEqualityComparison<TInput, TOutput>
    {
        public LiteralEqualityComparison(TOutput expected, bool equal = true) : base(i => expected, equal)
        {
        }
    }

    /// <summary>
    /// Used to compare a function output with an invariant value. This class
    /// supports both equality, less than, and greater than checks.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public class LiteralComparison<TInput, TOutput> : FunctionComparison<TInput, TOutput> where TOutput : IComparable<TOutput>
    {
        public LiteralComparison(TOutput expected, EqualityOperator equalityOperator) : base(i => expected, equalityOperator)
        {
        }
    }

    /// <summary>
    /// Used to combine comparisons with boolean operators.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public class BlockComparison<TInput, TOutput> : IComparison<TInput, TOutput>
    {
        private readonly IComparison<TInput, TOutput> _left;
        private readonly IComparison<TInput, TOutput> _right;
        private readonly BooleanOperator _op;

        public BlockComparison(IComparison<TInput, TOutput> left, IComparison<TInput, TOutput> right, BooleanOperator op)
        {
            _left = left;
            _right = right;
            _op = op;
        }

        public bool Matches(TInput inputSample, TOutput functionValue)
        {
            var leftMatch = _left.Matches(inputSample, functionValue);
            var rightMatch = _right.Matches(inputSample, functionValue);

            switch (_op)
            {
                case BooleanOperator.AND:
                    return leftMatch && rightMatch;
                case BooleanOperator.OR:
                    return leftMatch || rightMatch;
                default:
                    return false;
            }
        }
    }
}
