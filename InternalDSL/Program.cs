using InternalDSL.Builder;
using InternalDSL.Executor;
using static InternalDSL.Builder.Generators;
using static InternalDSL.Builder.FluentTestBuilder;

namespace InternalDSL
{
    class Program
    {
        static void Main(string[] args)
        {
            var sumTest =
                Test("Test of Sum function")
                    .Samples(100)
                    .Generator(
                        Pair(PosSmallInteger, PosSmallInteger))
                    .Property("Returns the sum of its input")
                        .Given(i => i.Item1 > 0)
                        .Given(i => i.Item2 > 0)
                        .Then(i => Add(i.Item1, i.Item2).I())
                        .Equals(i => i.Item1 + i.Item2)
                        .And()
                        .BeginBlock()
                            .BeginBlock()
                                .IsNotEqual(0)
                                .And()
                                .IsNotEqual(1)
                            .EndBlock()
                            .Or()
                            .BeginBlock()
                                .IsGreaterThan(1)
                                .Or()
                                .IsGreaterThan(0 + 1)
                                .Or()
                                .IsNotEqual(-1)
                            .EndBlock()
                        .EndBlock()
                        .And()
                        .IsNotEqual(i => i.Item1 - i.Item2)
                    .Property("Power (why not?)")
                        .Given(i => i.Item1 > 0)
                        .Then(i => AsString(i.Item1))
                        .IsNotEqual("0")
                        .Or()
                        .IsGreaterThan("0")
                    .Build();

            var executor = TestExecutor.Create(sumTest);
            executor.Assert();

            var sumTestLambdas =
                Test("Test of Sum function")
                    .Samples(100)
                    .Generator(
                        Pair(PosSmallInteger, PosSmallInteger))
                    .Property("Returns the sum of its input")
                    .Given(i => i.Item1 > 0)
                    .Given(i => i.Item2 > 0)
                    .ThenLambda(i => Add(i.Item1, i.Item2).I())
                    .Satisfies(b => b.And(
                        _ => b.Or(
                            __ => b.Equals(0),
                            __ => b.Equals(1)
                            ),
                        _ => b.IsNotEqual(6)
                    )).Build();
        }

        internal static int Add(int a, int b)
        {
            return a + b;
        }

        internal static string AsString(int a)
        {
            return $"{a}";
        }
    }
}
