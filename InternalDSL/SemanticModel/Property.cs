using System;

namespace InternalDSL.SemanticModel
{
    public interface IProperty<in TInput, out TOutput>
    {
        string Description { get; }

        Func<TInput, TOutput> Function { get; }

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
