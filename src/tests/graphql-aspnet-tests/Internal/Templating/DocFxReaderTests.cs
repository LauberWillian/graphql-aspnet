// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.IO;
    using System.Xml;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.DocFxTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DocFxReaderTests
    {
        [Test]
        public void SimpleTypeDescription_IsReadForExistingType()
        {
            var reader = new DocFXReader();
            var result = reader.ReadDescription(typeof(SimpleClassWithDocFxComments));

            var expected = "A comment for DocFx to read.";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SimpleMethodDescription_IsReadForExistingMethod()
        {
            var reader = new DocFXReader();
            var methodInfo = typeof(SimpleClassWithDocFxComments).GetMethod(nameof(SimpleClassWithDocFxComments.TestMethod));
            var result = reader.ReadDescription(methodInfo);

            var expected = "A comment on test method to read.";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SimplePropertyDescription_IsReadForExistingproperty()
        {
            var reader = new DocFXReader();
            var propInfo = typeof(SimpleClassWithDocFxComments).GetProperty(nameof(SimpleClassWithDocFxComments.TestProperty));
            var result = reader.ReadDescription(propInfo);

            var expected = "Gets or sets a comment on test property to read.";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SimpleMethodDescription_WithNoHeaderInfo_ReturnsNull()
        {
            var reader = new DocFXReader();
            var methodInfo = typeof(SimpleClassWithDocFxComments).GetMethod(nameof(SimpleClassWithDocFxComments.NoMethodSummary));
            var result = reader.ReadDescription(methodInfo);

            Assert.IsNull(result);
        }

        [Test]
        public void MethodWithGeneralInheritDocFromInterface_ReadsCorrectSummary()
        {
            var reader = new DocFXReader();
            var methodInfo = typeof(ImplementInterfaceClassWithDocFx)
                            .GetMethod(nameof(ImplementInterfaceClassWithDocFx.MethodFromInterface));

            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with a comment on an interface.";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MethodWithExplictInheritDocFromInterface_ReadsCorrectSummary()
        {
            var reader = new DocFXReader();
            var methodInfo = typeof(ImplementInterfaceClassWithDocFx)
                             .GetMethod(nameof(ImplementInterfaceClassWithDocFx.MethodFromInterface2));
            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with a comment on an interface2.";
            Assert.AreEqual(expected, result);
        }
    }
}