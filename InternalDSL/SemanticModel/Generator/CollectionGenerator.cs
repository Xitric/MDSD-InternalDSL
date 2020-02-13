using System.Collections.Generic;
using System.Linq;

namespace InternalDSL.SemanticModel.Generator
{
    public abstract class CollectionGenerator<T, TEnumerable> : Generator<TEnumerable> where TEnumerable : IEnumerable<T>
    {
        protected readonly Generator<T> Generator;

        protected CollectionGenerator(Generator<T> generator)
        {
            Generator = generator;
        }
    }

    public class ArrayGenerator<T> : CollectionGenerator<T, T[]>
    {
        public ArrayGenerator(Generator<T> generator) : base(generator)
        {
        }

        public override T[] Next()
        {
            var length = Rand.Next(256);
            var result = new T[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = Generator.Next();
            }

            return result;
        }
    }

    public class ListGenerator<T> : CollectionGenerator<T, IList<T>>
    {
        private readonly ArrayGenerator<T> _generator;

        public ListGenerator(Generator<T> generator) : base(generator)
        {
            _generator = new ArrayGenerator<T>(generator);
        }

        public override IList<T> Next()
        {
            return _generator.Next().ToList();
        }
    }
}
