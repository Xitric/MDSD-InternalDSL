using System;
using System.Threading;
using InternalDSL.Builder;
using InternalDSL.SemanticModel;
using InternalDSL.SemanticModel.Generator;
using static InternalDSL.SemanticModel.Generator.Generator;
using static InternalDSL.Builder.FluentTestBuilder;

namespace InternalDSL
{
    class Program
    {
        static void Main(string[] args)
        {
            var overZero = new LiteralComparison<(int, int), int>(0, EqualityOperator.GreaterThan);
            var sumEqual = new FunctionEqualityComparison<(int, int), int>(i => i.Item1 - i.Item2, true);

            var sumProperty = new Property<(int, int), int>("Returns the sum of its input", i => Add(i.Item1, i.Item2));
            sumProperty.AddComparison(sumEqual);

            var meaningfulProperty = new Property<(int, int), int>("Results make sense", i => Add(i.Item1, i.Item2));
            meaningfulProperty.AddComparison(overZero);

            var generator = new PairGenerator<int, int>(new PosSmallIntGenerator(), new PosSmallIntGenerator());
            var test = new Test<(int, int)>("Test of Sum function", 100, generator);
            test.AddProperty(sumProperty);
            test.AddProperty(meaningfulProperty);

            Console.WriteLine(test.Assert());
            Console.ReadKey();

            Test("Test of Sum function")
                .Generator(
                    Pair(PosSmallInteger, PosSmallInteger))
                .Property("Returns the sum of its input")
                .Given(i => i.Item1 > 0 && i.Item2 > 0)
                .Then(i => Add(i.Item1, i.Item2))
                .IsEqualTo(i => i.Item1 + i.Item2);
        }

        internal static int Add(int a, int b)
        {
            return a + b;
        }
    }
}
