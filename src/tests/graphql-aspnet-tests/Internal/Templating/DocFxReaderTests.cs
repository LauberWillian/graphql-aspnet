﻿// *************************************************************
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
    using System.Linq;
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
            var reader = new DocFxReader();
            var result = reader.ReadDescription(typeof(SimpleClassWithDocFxComments));

            var expected = "A comment for DocFx to read.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void SimpleMethodDescription_IsReadForExistingMethod()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(SimpleClassWithDocFxComments).GetMethod(nameof(SimpleClassWithDocFxComments.TestMethod));
            var result = reader.ReadDescription(methodInfo);

            var expected = "A comment on test method to read.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void SimplePropertyDescription_IsReadForExistingproperty()
        {
            var reader = new DocFxReader();
            var propInfo = typeof(SimpleClassWithDocFxComments).GetProperty(nameof(SimpleClassWithDocFxComments.TestProperty));
            var result = reader.ReadDescription(propInfo);

            var expected = "Gets or sets a comment on test property to read.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void SimpleMethodDescription_WithNoHeaderInfo_ReturnsNull()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(SimpleClassWithDocFxComments).GetMethod(nameof(SimpleClassWithDocFxComments.NoMethodSummary));
            var result = reader.ReadDescription(methodInfo);

            Assert.IsNull(result);

            reader.Dispose();
        }

        [Test]
        public void MethodWithGeneralInheritDocFromInterface_ReadsCorrectSummary()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ImplementInterfaceClassWithDocFx)
                            .GetMethod(nameof(ImplementInterfaceClassWithDocFx.MethodFromInterface));

            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with a comment on an interface.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void MethodWithExplictInheritDocFromInterface_ReadsCorrectSummary()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ImplementInterfaceClassWithDocFx)
                             .GetMethod(nameof(ImplementInterfaceClassWithDocFx.MethodFromInterface2));
            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with a comment on an interface2.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void WhenBaseClassImplementsDoc_AndChildClassHasInheritDoc_BaseDocIsUsed()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ChildClassWithInheritDoc)
                             .GetMethod(nameof(ChildClassWithInheritDoc.MethodToCall));
            var result = reader.ReadDescription(methodInfo);

            var expected = "This is the methodToCall description.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void MethodWithOneSimpleParam_FindsDocumentationStringCorrectly()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ClassWithMethodWithParam)
                             .GetMethod(nameof(ClassWithMethodWithParam.MethodWithOneParam));
            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with one parameter.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void SingleParameterOfMethod_FindsDocumentationStringCorrectly()
        {
            var reader = new DocFxReader();
            var paramInfo = typeof(ClassWithMethodWithParam)
                             .GetMethod(nameof(ClassWithMethodWithParam.MethodWithOneParam))
                             .GetParameters().First(x => string.Compare(x.Name, "firstParam", System.StringComparison.OrdinalIgnoreCase) == 0);

            var result = reader.ReadDescription(paramInfo);

            var expected = "This is the first parameter called firstParam.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void MultiParamMethod_FindsDocumentationStringCorrectly()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ClassWithMultiParamMethod)
                             .GetMethod(nameof(ClassWithMultiParamMethod.MultiParamMethod));
            var result = reader.ReadDescription(methodInfo);

            var expected = "This is a method with three parameters.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void MultiParamMethod_NotFirstParam_FindsDocumentationStringCorrectly()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ClassWithMultiParamMethod)
                             .GetMethod(nameof(ClassWithMultiParamMethod.MultiParamMethod))
                             .GetParameters()
                             .Single(x => string.Compare(x.Name, "secondParam", System.StringComparison.OrdinalIgnoreCase) == 0);

            var result = reader.ReadDescription(methodInfo);

            var expected = "The 2nd parameter called second param.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }

        [Test]
        public void SingleGengericParamMethod_ReturnsExceptedSummary()
        {
            var reader = new DocFxReader();
            var methodInfo = typeof(ClassWith2ParamGenericMethod).GetMethods().Single();

            var result = reader.ReadDescription(methodInfo);

            var expected = "Generic Method Summary for two params is here.";
            Assert.AreEqual(expected, result);

            reader.Dispose();
        }
    }
}