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
        private readonly Func<TInput, TOutput> _function;
        private readonly IList<Comparison<TInput, TOutput>> _comparisons;

        public Property(Func<TInput, TOutput> function)
        {
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
        public static IProperty<TInput> Make<TOutput>(Func<TInput, TOutput> f)
        {
            return new Property<TInput, TOutput>(f);
        }
    }
}
