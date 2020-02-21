using System;
using System.Text;

namespace InternalDSL.SemanticModel.Generator
{
    /// <summary>
    /// Superclass of all types that can be used to generate random input
    /// values to properties in tests.
    /// </summary>
    /// <typeparam name="T">The type of random value to generate</typeparam>
    public abstract class Generator<T>
    {
        protected static readonly Random Rand = new Random();

        public abstract T Next();
    }

    /// <summary>
    /// Generates random integers.
    /// </summary>
    public class IntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next() - (int.MaxValue / 2);
    }

    /// <summary>
    /// Generates random positive integers.
    /// </summary>
    public class PosIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next();
    }

    /// <summary>
    /// Generates random shorts.
    /// </summary>
    public class SmallIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next(short.MaxValue) - (short.MaxValue / 2);
    }

    /// <summary>
    /// Generates random positive shorts.
    /// </summary>
    public class PosSmallIntGenerator : Generator<int>
    {
        public override int Next() => Rand.Next(short.MaxValue);
    }

    /// <summary>
    /// Generates random printable characters.
    /// </summary>
    public class CharGenerator : Generator<char>
    {
        public override char Next() => (char)Rand.Next(32, 128);
    }

    /// <summary>
    /// Generates random doubles.
    /// </summary>
    public class DoubleGenerator : Generator<double>
    {
        public override double Next() => Rand.NextDouble();
    }

    /// <summary>
    /// Generates random floats.
    /// </summary>
    public class FloatGenerator : Generator<float>
    {
        public override float Next() => (float)Rand.NextDouble();
    }

    /// <summary>
    /// Generates random variable-length strings of printable characters.
    /// </summary>
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
