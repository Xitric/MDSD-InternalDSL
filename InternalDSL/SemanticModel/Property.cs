using System;
using System.Collections.Generic;
using System.Linq;

namespace InternalDSL.SemanticModel
{
    public interface IProperty<in TInput>
    {
        string Description { get; }

        bool Assert(TInput input);
    }

    public class Property<TInput, TOutput> : IProperty<TInput>
    {
        public string Description { get; }
        private readonly Func<TInput, TOutput> _function;
        private readonly IList<IComparison<TInput, TOutput>> _comparisons;

        public Property(string description, Func<TInput, TOutput> function)
        {
            Description = description;
            _function = function;
            _comparisons = new List<IComparison<TInput, TOutput>>();
        }

        public void AddComparison(IComparison<TInput, TOutput> comparison)
        {
            _comparisons.Add(comparison);
        }

        public bool Assert(TInput input)
        {
            var functionValue = _function(input);
            return _comparisons.All(comparison => comparison.Matches(input, functionValue));
        }
    }
}
