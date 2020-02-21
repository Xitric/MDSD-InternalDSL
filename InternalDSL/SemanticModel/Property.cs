using System;

namespace InternalDSL.SemanticModel
{
    /// <summary>
    /// A property is a predicate that must be true for all input values
    /// generated during a test.
    /// </summary>
    /// <typeparam name="TInput">The type of input given to the function under test</typeparam>
    /// <typeparam name="TOutput">The return type of the function under test</typeparam>
    public interface IProperty<in TInput, out TOutput>
    {
        /// <summary>
        /// A human-readable description of this property, printed in case it
        /// is violated.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The function under test.
        /// </summary>
        Func<TInput, TOutput> Function { get; }

        /// <summary>
        /// A compound boolean expression that must be true for all input
        /// values provided to the function under test.
        /// </summary>
        IComparison Comparison { get; }
    }

    public class Property<TInput, TOutput> : IProperty<TInput, TOutput>
    {
        public string Description { get; }
        public Func<TInput, TOutput> Function { get; }
        public IComparison Comparison { get; }

        public Property(string description, Func<TInput, TOutput> function, IComparison comparison)
        {
            Description = description;
            Function = function;
            Comparison = comparison;
        }
    }
}
