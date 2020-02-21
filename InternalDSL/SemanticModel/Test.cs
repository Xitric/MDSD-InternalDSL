using System.Collections.Generic;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.SemanticModel
{
    public interface ITest<TInput>
    {
        string Name { get; }

        int Samples { get; }

        Generator<TInput> Generator { get; }

        IList<IProperty<TInput, dynamic>> Properties { get; }
    }

    public class Test<TInput> : ITest<TInput>
    {
        public string Name { get; }
        public int Samples { get; }
        public Generator<TInput> Generator { get; }
        public IList<IProperty<TInput, dynamic>> Properties { get; }

        public Test(string name, int samples, Generator<TInput> generator)
        {
            Name = name;
            Samples = samples;
            Generator = generator;
            Properties = new List<IProperty<TInput, dynamic>>();
        }

        internal void AddProperty(IProperty<TInput, dynamic> property)
        {
            Properties.Add(property);
        }
    }
}
