using InternalDSL.Builder;
using InternalDSL.Executor;
using static InternalDSL.Builder.Generators;
using static InternalDSL.Builder.FluentTestBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InternalDSLTests
{
    [TestClass]
    public class TestEqualityTest
    {
        [TestMethod]
        public void TestDifferentDsl()
        {
            //Create a test using method chaining
            var testA =
                Test("A cool test")
                    .Samples(250)
                    .Generator(
                        Pair(Triplet(Generators.Character, Str, Generators.Float), Array(PosSmallInteger)))
                    .Property("A property")
                        .Then(i => (1 + 2).I())
                        .BeginBlock()
                            .Equals(i => 1 + 2)
                            .Or()
                            .Equals(i => 2 + 1)
                        .EndBlock()
                        .And()
                        .BeginBlock()
                            .Equals(0)
                            .And()
                            .BeginBlock()
                                .IsNotEqual(int.MaxValue)
                                .Or()
                                .IsNotEqual(1)
                            .EndBlock()
                        .EndBlock()
                    .Property("Is not the difference")
                        .Then(i => (1 + 2).I())
                        .IsNotEqual(i => 1 - 2)
                    .Build();

            //Create the exact same test using nested lambdas
            var testB =
                Test("A cool test")
                    .Samples(250)
                    .Generator(
                        Pair(Triplet(Generators.Character, Str, Generators.Float), Array(PosSmallInteger)))
                    .Property("A property")
                        .ThenLambda(i => (1 + 2).I())
                        .Satisfies(b => b.And(
                                _ => b.Or(
                                    __ => b.Equals(i => 1 + 2),
                                    __ => b.Equals(i => 2 + 1)
                                ),
                                _ => b.And(
                                    __ => b.Equals(0),
                                    __ => b.Or(
                                        ___ => b.IsNotEqual(int.MaxValue),
                                        ___ => b.IsNotEqual(1)
                                    )
                                )
                            )
                        )
                    .Property("Is not the difference")
                        .ThenLambda(i => (1 + 2).I())
                        .Satisfies(b => b.IsNotEqual(i => 1 - 2))
                    .Build();

            var validator = TestValidator.Create(testA, testB);
            validator.Assert();
        }
    }
}
