﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    public class SimpleScalarObjectGraphType : BaseScalarType
    {
        public SimpleScalarObjectGraphType()
            : base(nameof(SimpleObjectScalar), typeof(SimpleObjectScalar))
        {
            this.Description = "Scalar from a object";
        }

        public override ScalarValueType ValueType => ScalarValueType.String;

        public override TypeCollection OtherKnownTypes { get; } = new TypeCollection();

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override object Serialize(object item)
        {
            return null;
        }
    }
}