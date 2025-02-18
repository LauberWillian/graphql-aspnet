﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.Subscriptions.Tests.Defaults.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class DirectiveWithArgs : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult Execute(int arg1, string arg2)
        {
            return null;
        }
    }
}