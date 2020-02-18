using System;
using System.Text;

namespace InternalDSL.SemanticModel.Generator
{
    public abstract class Generator<T>
    {
        protected static readonly Random Rand = new Random();

        public abstract T Next();
    }

    public class IntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next() - (int.MaxValue / 2);
    }

    public class PosIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next();
    }

    public class SmallIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next(short.MaxValue) - (short.MaxValue / 2);
    }

    public class PosSmallIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next(short.MaxValue);
    }

    public class CharGenerator : Generator<char>
    {
        public override char Next() => (char)Rand.Next(32, 128);
    }

    public class DoubleGenerator : Generator<double>
    {
        public override double Next() => Rand.NextDouble();
    }

    public class FloatGenerator : Generator<float>
    {
        public override float Next() => (float)Rand.NextDouble();
    }

    public class StringGenerator : Generator<string>
    {
        private readonly CharGenerator _charGenerator = new CharGenerator();

        public override string Next()
        {
            var length = Rand.Next(256);
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                result.Append(_charGenerator.Next());
            }

            return result.ToString();
        }
    }
}
