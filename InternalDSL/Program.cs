using System.Linq;
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
            //A number of more or less silly examples of tests showing the generality of the DSL
            //More examples can be found in the unit test project
            var simpleAddTest =
                Test("Test of Add method")
                    .Generator(
                        Pair(PosSmallInteger, PosSmallInteger))
                    .Property("Returns the sum of its input")
                        .Then(i => Add(i.Item1, i.Item2).I())
                        .Equals(i => i.Item1 + i.Item2)
                    .Build();

            var simpleConcatTest =
                Test("Test of Union method")
                    .Generator(Triplet(Array(Str), Array(Str), Array(Str)))
                        .Property("Union keeps correct length")
                        .Given(i => i.Item1 != null)
                        .Given(i => i.Item2 != null)
                        .Given(i => i.Item3 != null)
                        .Then(i => i.Item1.Concat(i.Item2).Concat(i.Item3).Count().I())
                        .Equals(i => i.Item1.Length + i.Item2.Length + i.Item3.Length)
                        .And()
                        .IsGreaterThan(i => i.Item1.Length)
                        .And()
                        .IsGreaterThan(i => i.Item2.Length)
                        .And()
                        .IsGreaterThan(i => i.Item3.Length)
                    .Property("Last element is correct")
                        .Given(i => i.Item2.Length > 0)
                        .Then(i => i.Item1.Union(i.Item2).Last())
                        .IsNotEqual((string) null)
                        .And()
                        .Equals(i => i.Item2.Last())
                    .Build();

            var sumTest =
                Test("Test of Add function")
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
                    .Build();

            var sumTestLambdas =
                Test("Test of Add function")
                    .Samples(1000)
                    .Generator(
                        Pair(PosSmallInteger, PosSmallInteger))
                    .Property("Returns the sum of its input")
                        .Given(i => i.Item1 > 0)
                        .Given(i => i.Item2 > 0)
                        .ThenLambda(i => Add(i.Item1, i.Item2).I())
                        .Satisfies(b => b.And(
                            _ => b.Or(
                                __ => b.Equals(i => i.Item1 + i.Item2),
                                __ => b.Equals(i => i.Item2 + i.Item1)
                                ),
                            _ => b.And(
                                __ => b.IsNotEqual(0),
                                __ => b.Or(
                                    ___ => b.IsNotEqual(int.MaxValue),
                                    ___ => b.IsNotEqual(1)
                                    )
                                )
                            ))
                    .Build();

            //To be executed in a Unit test project
            //var executor = TestExecutor.Create(sumTestLambdas);
            //executor.Assert();
        }

        private static int Add(int a, int b)
        {
            return a + b;
        }
    }
}
