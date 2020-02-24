using System;
using static InternalDSL.Builder.Generators;
using static InternalDSL.Builder.FluentTestBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InternalDSLTests
{
    [TestClass]
    public class FailedComparisonsTest
    {
        [TestMethod]
        public void TestMissingEndBlock()
        {
            var invalidBuilder =
                Test("Invalid test")
                    .Generator(Str)
                    .Property("Invalid property")
                        .Then(i => "")
                        .BeginBlock()
                            .BeginBlock()
                                .Equals("")
                            .EndBlock();
            Assert.ThrowsException<InvalidOperationException>(invalidBuilder.Build);
        }

        [TestMethod]
        public void TestMissingBeginBlock()
        {
            var invalidBuilder =
                Test("Invalid test")
                    .Generator(Str)
                    .Property("Invalid property")
                        .Then(i => "")
                        .BeginBlock()
                            .Equals("")
                        .EndBlock();
            Assert.ThrowsException<InvalidOperationException>(invalidBuilder.EndBlock);
        }
    }
}
