using System;

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
    /// Used tp configure how predicates are combined into more complex
    /// predicates.
    /// </summary>
    public enum BooleanOperator
    {
        And = 0,
        Or = 1
    }

    /// <summary>
    /// Instances of this class are used to express expectations on return
    /// values from functions under test. In the context of propositions, a
    /// comparison can be thought of as the propositional function.
    /// </summary>
    public interface IComparison
    {
    }

    /// <summary>
    /// Used to compare the output of the function under test with the output
    /// of another function for equality or inequality only.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    public class FunctionEqualityComparison<TInput> : IComparison
    {
        public Func<TInput, dynamic> ExpectedFunction { get; }
        public bool CheckEqual { get; }

        public FunctionEqualityComparison(Func<TInput, dynamic> expectedFunction, bool equal = true)
        {
            ExpectedFunction = expectedFunction;
            CheckEqual = equal;
        }
    }

    /// <summary>
    /// Used to compare the output of the function under test with the output
    /// of another function. This class supports both equality, less than, and
    /// greater than checks.
    /// </summary>
    public class FunctionComparison<TInput> : FunctionEqualityComparison<TInput>
    {
        public EqualityOperator EqualityOperator { get; }

        public FunctionComparison(Func<TInput, dynamic> expectedFunction, EqualityOperator equalityOperator) : base(expectedFunction)
        {
            EqualityOperator = equalityOperator;
        }
    }

    /// <summary>
    /// Used to compare a function output with an invariant value for equality
    /// or inequality only.
    /// </summary>
    public class LiteralEqualityComparison<TInput> : FunctionEqualityComparison<TInput>
    {
        public LiteralEqualityComparison(dynamic expected, bool equal = true) : base(i => expected, equal)
        {
        }
    }

    /// <summary>
    /// Used to compare a function output with an invariant value. This class
    /// supports both equality, less than, and greater than checks.
    /// </summary>
    public class LiteralComparison<TInput> : FunctionComparison<TInput>
    {
        public LiteralComparison(dynamic expected, EqualityOperator equalityOperator) : base(i => expected, equalityOperator)
        {
        }
    }

    /// <summary>
    /// Used to combine comparisons with boolean operators.
    /// </summary>
    public class BlockComparison : IComparison
    {
        public IComparison Left { get; }
        public IComparison Right { get; }
        public BooleanOperator Operator { get; }

        public BlockComparison(IComparison left, IComparison right, BooleanOperator op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
    }
}
