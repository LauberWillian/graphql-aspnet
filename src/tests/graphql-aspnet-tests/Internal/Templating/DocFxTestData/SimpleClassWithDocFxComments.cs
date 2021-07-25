// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.DocFxTestData
{
    /// <summary>
    /// A comment for DocFx to read.
    /// </summary>
    public class SimpleClassWithDocFxComments
    {
        /// <summary>
        /// A comment on test method to read.
        /// </summary>
        /// <returns>System.String.</returns>
        public string TestMethod()
        {
            return null;
        }

        public string NoMethodSummary()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets a comment on test property to read.
        /// </summary>
        /// <value>The test property.</value>
        public string TestProperty { get; set; }
    }
}