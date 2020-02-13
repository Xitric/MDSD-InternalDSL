using System;
using System.Collections.Generic;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.SemanticModel
{
    public class Test<TInput>
    {
        public string Name { get; }
        private readonly int _samples;
        private readonly Generator<TInput> _generator;
        private readonly IList<IProperty<TInput>> _properties;

        public Test(string name, int samples, Generator<TInput> generator)
        {
            Name = name;
            _samples = samples;
            _generator = generator;
            _properties = new List<IProperty<TInput>>();
        }

        public void AddProperty(IProperty<TInput> property)
        {
            _properties.Add(property);
        }

        //TODO: Make test fail or pass instead of returning bool - this is only for testing
        public bool Assert()
        {
            for (var i = 0; i < _samples; i++)
            {
                if (!AssertProperties())
                {
                    return false;
                }
            }

            return true;
        }

        private bool AssertProperties()
        {
            var input = _generator.Next();
            foreach (var property in _properties)
            {
                if (property.Assert(input)) continue;

                //TODO: Fail test with message
                Console.WriteLine($"FAILED: \"{Name}\" for value {input}");
                Console.WriteLine($"Failed property \"{property.Description}\"");
                return false;
            }

            return true;
        }
    }
}
