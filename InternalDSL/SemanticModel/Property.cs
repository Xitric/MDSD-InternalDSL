using System;

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
        private readonly IComparison<TInput, TOutput> _comparison;

        public Property(string description, Func<TInput, TOutput> function, IComparison<TInput, TOutput> comparison)
        {
            Description = description;
            _function = function;
            _comparison = comparison;
        }

        public bool Assert(TInput input)
        {
            var functionValue = _function(input);
            return _comparison.Matches(input, functionValue);
        }
    }
}
