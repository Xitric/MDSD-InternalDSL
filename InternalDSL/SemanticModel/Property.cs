using System;
using System.Collections.Generic;
using System.Linq;

namespace InternalDSL.SemanticModel
{
    public interface IProperty<in TInput>
    {
        bool Assert(TInput input);
    }

    internal class Property<TInput, TOutput> : IProperty<TInput>
    {
        public string Description { get; }
        private readonly Func<TInput, TOutput> _function;
        private readonly IList<Comparison<TInput, TOutput>> _comparisons;

        public Property(string description, Func<TInput, TOutput> function)
        {
            Description = description;
            _function = function;
            _comparisons = new List<Comparison<TInput, TOutput>>();
        }

        public void AddComparison(Comparison<TInput, TOutput> comparison)
        {
            _comparisons.Add(comparison);
        }

        public bool Assert(TInput input)
        {
            var functionValue = _function(input);
            return _comparisons.All(comparison => comparison.Matches(input, functionValue));
        }
    }

    public class Property<TInput>
    {
        public static IProperty<TInput> Make<TOutput>(string description, Func<TInput, TOutput> function)
        {
            return new Property<TInput, TOutput>(description, function);
        }
    }
}
