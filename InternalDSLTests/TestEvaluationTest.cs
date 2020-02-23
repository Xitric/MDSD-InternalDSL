using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InternalDSL.Executor;
using InternalDSL.Builder;
using static InternalDSL.Builder.Generators;
using static InternalDSL.Builder.FluentTestBuilder;

namespace InternalDSLTests
{
    [TestClass]
    public class TestEvaluationTest
    {
        [TestMethod]
        public void SumTest()
        {
            //Create a test that is supposed to pass
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
        }

        [TestMethod]
        public void MoreTests()
        {
            var simpleAddTest =
                Test("Test of Add method")
                    .Generator(
                        Pair(PosSmallInteger, PosSmallInteger))
                    .Property("Returns the sum of its input")
                        .Then(i => Add(i.Item1, i.Item2).I())
                        .Equals(i => i.Item1 + i.Item2)
                    .Build();
            TestExecutor.Create(simpleAddTest).Assert();

            var simpleConcatTest =
                Test("Test of Union method")
                    .Generator(Triplet(Array(Str), Array(Str), Array(Str)))
                    .Property("Union keeps correct length")
                        .Given(i => i.Item1.Any())
                        .Given(i => i.Item2.Any())
                        .Given(i => i.Item3.Any())
                        .Then(i => i.Item1.Concat(i.Item2).Concat(i.Item3).Count().I())
                        .Equals(i => i.Item1.Length + i.Item2.Length + i.Item3.Length)
                        .And()
                        .IsGreaterThan(i => i.Item1.Length)
                        .And()
                        .IsGreaterThan(i => i.Item2.Length)
                        .And()
                        .IsGreaterThan(i => i.Item3.Length)
                    .Property("Last element is correct")
                        .Given(i => i.Item2.Any())
                        .Then(i => i.Item1.Concat(i.Item2).Last())
                        .IsNotEqual((string)null)
                        .And()
                        .Equals(i => i.Item2.Last())
                    .Build();
            TestExecutor.Create(simpleConcatTest).Assert();

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
            TestExecutor.Create(sumTest).Assert();

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
            TestExecutor.Create(sumTestLambdas).Assert();
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        internal string AsString(int a)
        {
            return $"{a}";
        }
    }
}
