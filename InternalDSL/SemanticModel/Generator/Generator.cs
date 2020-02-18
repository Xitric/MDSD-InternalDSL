using System;
using System.Text;

namespace InternalDSL.SemanticModel.Generator
{
    //TODO: Move this to the DSL part
    public class Generator
    {
        public static readonly IntGenerator Integer = new IntGenerator();
        public static readonly PosIntGenerator PosInteger = new PosIntGenerator();
        public static readonly SmallIntGenerator SmallInteger = new SmallIntGenerator();
        public static readonly PosSmallIntGenerator PosSmallInteger = new PosSmallIntGenerator();
        public static readonly CharGenerator Character = new CharGenerator();
        public static readonly DoubleGenerator Double = new DoubleGenerator();
        public static readonly FloatGenerator Float = new FloatGenerator();
        public static readonly StringGenerator Str = new StringGenerator();
        public static ArrayGenerator<T> Array<T>(Generator<T> generator) => new ArrayGenerator<T>(generator);
        public static ListGenerator<T> List<T>(Generator<T> generator) => new ListGenerator<T>(generator);
        public static PairGenerator<T1, T2> Pair<T1, T2>(Generator<T1> generator1, Generator<T2> generator2) => new PairGenerator<T1, T2>(generator1, generator2);
        public static TripletGenerator<T1, T2, T3> Triplet<T1, T2, T3>(Generator<T1> generator1, Generator<T2> generator2, Generator<T3> generator3) => new TripletGenerator<T1, T2, T3>(generator1, generator2, generator3);
    }

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
