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
    using System;

    public class ImplementInterfaceClassWithDocFx : IDocFxInterface, IDocFxInterface2
    {
        /// <inheritdoc />
        public string MethodFromInterface()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IDocFxInterface2.MethodFromInterface2" />
        public string MethodFromInterface2()
        {
            throw new NotImplementedException();
        }
    }
}