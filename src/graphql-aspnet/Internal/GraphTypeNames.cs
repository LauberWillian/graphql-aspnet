﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A global collection of graph type names and their concrete type references.
    /// </summary>
    public static class GraphTypeNames
    {
        private static readonly Dictionary<Type, ConcurrentDictionary<TypeKind, string>> KNOWN_NAMES
            = new Dictionary<Type, ConcurrentDictionary<TypeKind, string>>();

        /// <summary>
        /// Initializes static members of the <see cref="GraphTypeNames"/> class.
        /// </summary>
        static GraphTypeNames()
        {
        }

        private static ConcurrentDictionary<TypeKind, string> RetrieveTypeDictionary(Type type)
        {
            lock (KNOWN_NAMES)
            {
                if (!KNOWN_NAMES.ContainsKey(type))
                {
                    KNOWN_NAMES.Add(type, new ConcurrentDictionary<TypeKind, string>());
                }

                return KNOWN_NAMES[type];
            }
        }

        /// <summary>
        /// Parses the name of the type as it would exist in the object graph.
        /// </summary>
        /// <typeparam name="TType">The concrete type to retrieve the graph type name of.</typeparam>
        /// <param name="kind">The kind of type to generate a name for.</param>
        /// <returns>System.String.</returns>
        public static string ParseName<TType>(TypeKind kind)
        {
            return ParseName(typeof(TType), kind);
        }

        /// <summary>
        /// Forciably assigns a graph name to a type, will override any previously assigned name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="kind">The kind of object for which the name will be assigned.</param>
        /// <param name="graphTypeName">Name of the graph type.</param>
        public static void AssignName(Type type, TypeKind kind, string graphTypeName)
        {
            Validation.ThrowIfNull(type, nameof(type));

            if (!GraphValidation.IsValidGraphName(graphTypeName))
            {
                throw new GraphTypeDeclarationException(
                    $"The type, '{type.FriendlyName()}', declares an invalid graph type name, '{graphTypeName}'. Graph type names " +
                    "can only contain letters A-Z, numbers 0-9 and an underscore. They must also not start with a double underscore.",
                    type);
            }

            var typeNameDictionary = RetrieveTypeDictionary(type);
            typeNameDictionary.AddOrUpdate(kind, graphTypeName, (_, __) => graphTypeName);
        }

        /// <summary>
        /// Parses the name of the type as it would exist in the object graph
        /// and returns the name. Schema name formatting rules are not applied during
        /// this method.
        /// </summary>
        /// <param name="type">The concrete type to parse a name from.</param>
        /// <param name="kind">The kind of graph type being created.</param>
        /// <returns>System.String.</returns>
        public static string ParseName(Type type, TypeKind kind)
        {
            Validation.ThrowIfNull(type, nameof(type));

            kind = GraphValidation.ResolveTypeKind(type, kind);
            var typeNameDictionary = RetrieveTypeDictionary(type);
            if (typeNameDictionary.TryGetValue(kind, out var typeName))
                return typeName;

            type = GraphValidation.EliminateWrappersFromCoreType(type);
            if (GraphQLProviders.ScalarProvider.IsScalar(type))
            {
                typeName = GraphQLProviders.ScalarProvider.RetrieveScalarName(type);
            }
            else if (type.IsEnum)
            {
                // enums always are their declared name (no changes needed for input types)
                typeName = type.SingleAttributeOrDefault<GraphTypeAttribute>()?.Name;
            }
            else if (kind == TypeKind.INPUT_OBJECT)
            {
                var inputNameAttrib = type.SingleAttributeOrDefault<GraphTypeAttribute>();
                if (inputNameAttrib != null && !string.IsNullOrWhiteSpace(inputNameAttrib.InputName))
                {
                    ThrowIfNamedGenericType(type, inputNameAttrib);
                    typeName = inputNameAttrib.InputName;
                }
                else
                {
                    typeName = GraphTypeNames.ParseName(type, TypeKind.OBJECT);
                    typeName = $"{Constants.CommonPrefix.INPUT_OBJECT_NAME_PREFIX}{typeName}";
                }
            }
            else
            {
                var graphTypeNameAttrib = type.SingleAttributeOrDefault<GraphTypeAttribute>();
                if (graphTypeNameAttrib != null)
                {
                    ThrowIfNamedGenericType(type, graphTypeNameAttrib);
                    typeName = graphTypeNameAttrib.Name;
                }
            }

            typeName = typeName ?? type.FriendlyGraphTypeName();
            typeName = typeName.Replace(Constants.Routing.CLASS_META_NAME, type.Name).Trim();
            if (kind == TypeKind.DIRECTIVE && typeName.EndsWith(Constants.CommonSuffix.DIRECTIVE_SUFFIX))
            {
                typeName = typeName.ReplaceLastInstanceOfCaseInvariant(Constants.CommonSuffix.DIRECTIVE_SUFFIX, string.Empty);
            }

            AssignName(type, kind, typeName);
            return typeName;
        }

        private static void ThrowIfNamedGenericType(Type type, GraphTypeAttribute attrib)
        {
            if (type.IsGenericType)
            {
                var name = attrib.Name;
                var inputName = attrib.InputName;
                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(inputName))
                {
                    string combinedName;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(inputName))
                        combinedName = $"'{name}' or '{inputName}'";
                    else if (!string.IsNullOrWhiteSpace(name))
                        combinedName = $"'{name}'";
                    else
                        combinedName = $"'{inputName}'";

                    var genericDef = type.GetGenericTypeDefinition();
                    throw new GraphTypeDeclarationException(
                        $"Generic Types such as '{genericDef.FriendlyName()}', cannot use the '{nameof(GraphTypeAttribute)}' " +
                        $"to declare a graph type name (e.g. {combinedName}).  Doing so could result in a single common name across multiple distinct types at runtime. " +
                        "Remove the declared name and try again.",
                        type);
                }
            }
        }

        /// <summary>
        /// Creates friendly name for a type that can be considered a valid name of a GraphQL graph type.
        /// This method does not take into account any scheme specific naming or casing rules.
        /// </summary>
        /// <param name="type">The type to create a friendly Name for.</param>
        /// <returns>System.String.</returns>
        public static string FriendlyGraphTypeName(this Type type)
        {
            return type.FriendlyName("_");
        }
    }
}