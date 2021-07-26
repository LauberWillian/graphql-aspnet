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
    public class ChildClassWithInheritDoc : BaseClassWithDocFxComments
    {
        /// <inheritdoc />
        public override void MethodToCall()
        {
            base.MethodToCall();
        }
    }
}