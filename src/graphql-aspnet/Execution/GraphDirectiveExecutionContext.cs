// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A set of information needed to successifully execute a directive as part of a field resolution.
    /// </summary>
    [DebuggerDisplay("Directive Context: {Directive.Name}")]
    public class GraphDirectiveExecutionContext : IDirectiveInvocationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveExecutionContext"/> class.
        /// </summary>
        /// <param name="seenAtLocation">Where in the source document the directive was seen.</param>
        /// <param name="graphType">The graph type being targeted.</param>
        /// <param name="origin">The location in the source document being executed as part of this context.</param>
        public GraphDirectiveExecutionContext(DirectiveLocation seenAtLocation, IDirectiveGraphType graphType, SourceOrigin origin)
        {
            this.Location = seenAtLocation;
            this.Directive = graphType;
            this.Origin = origin;
            this.Arguments = new InputArgumentCollection();
        }

        /// <inheritdoc />
        public DirectiveLocation Location { get; }

        /// <inheritdoc />
        public IDirectiveGraphType Directive { get; }

        /// <inheritdoc />
        public SourceOrigin Origin { get; }

        /// <inheritdoc />
        public IInputArgumentCollection Arguments { get; }
    }
}