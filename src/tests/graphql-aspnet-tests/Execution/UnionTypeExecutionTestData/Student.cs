﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.UnionTypeExecutionTestData
{
    public class Student : Person
    {
        public string ParentsName { get; set; }
    }
}