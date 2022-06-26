﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentPartsNew;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A document representing the query text as supplied by the user matched against a schema.
    /// </summary>
    internal class QueryDocument : IGraphQueryDocument
    {
        private readonly Dictionary<string, IOperationDocumentPart> _operations;
        private readonly DocumentNamedFragmentCollection _fragmentCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDocument" /> class.
        /// </summary>
        public QueryDocument()
        {
            this.Messages = new GraphMessageCollection();
            this.Children = new DocumentPartsCollection(this);

            this.Path = new SourcePath();
            this.Path.AddFieldName("document");

            _fragmentCollection = new DocumentNamedFragmentCollection(this);

            this.Children.PartAdded += this.Children_PartAdded;
        }

        private void Children_PartAdded(object sender, DocumentPartEventArgs eventArgs)
        {
            if (eventArgs.TargetDocumentPart is INamedFragmentDocumentPart nf)
                _fragmentCollection.AddFragment(nf);
        }

        /// <inheritdoc />
        public void AssignGraphType(IGraphType graphType)
        {
            throw new System.NotSupportedException("No graph type exists that can used for the document");
        }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public int MaxDepth { get; set; }

        /// <inheritdoc />
        public IDocumentPartsCollection Children { get; }

        /// <inheritdoc />
        public IGraphType GraphType => null;

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.Document;

        /// <inheritdoc />
        public IDocumentPart Parent => null;

        /// <inheritdoc />
        public SyntaxNode Node => EmptyNode.Instance;

        /// <inheritdoc />
        public SourcePath Path { get; }

        /// <inheritdoc />
        public IReadOnlyList<IOperationDocumentPart> Operations =>
            this.Children[DocumentPartType.Operation]
            .OfType<IOperationDocumentPart>()
            .ToList();

        /// <inheritdoc />
        public INamedFragmentCollectionDocumentPart NamedFragments => _fragmentCollection;
    }
}