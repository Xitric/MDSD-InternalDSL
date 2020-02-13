using System;
using InternalDSL.SemanticModel;
using InternalDSL.SemanticModel.Generator;

namespace InternalDSL
{
    class Program
    {
        static void Main(string[] args)
        {
            var overZero = new LiteralComparison<(int, int), int>(0, ComparisonType.GreaterThan);
            var sumEqual = new FunctionEqualityComparison<(int, int), int>(i => i.Item1 + i.Item2);

            var sumProperty = new Property<(int, int), int>("Returns the sum of its input", i => Add(i.Item1, i.Item2));
            sumProperty.AddComparison(sumEqual);

            var meaningfulProperty = new Property<(int, int), int>("Results make sense", i => Add(i.Item1, i.Item2));
            meaningfulProperty.AddComparison(overZero);

            var generator = new PairGenerator<int, int>(new PosSmallIntGenerator(), new PosSmallIntGenerator());
            var test = new Test<(int, int)>("Test of sum function", 100, generator);
            test.AddProperty(sumProperty);
            test.AddProperty(meaningfulProperty);

            Console.WriteLine(test.Assert());
            Console.ReadKey();
        }

        internal static int Add(int a, int b)
        {
            return a + b;
        }
    }
}
