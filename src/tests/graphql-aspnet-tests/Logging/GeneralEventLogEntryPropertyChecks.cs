﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.ExecutionEvents;
    using GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.ServerBuilders;
    using GraphQL.AspNet.Tests.Logging.LoggerTestData;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Ensure that the getters/setters that map to property name constants
    /// are storing and retrieving from the same keys.
    /// </summary>
    [TestFixture]
    public class GeneralEventLogEntryPropertyChecks
    {
        [Test]
        public void SchemaInstanceCreatedLogEntry()
        {
            var server = new TestServerBuilder()
                .AddType<LogTestController>()
                .Build();

            var entry = new SchemaInstanceCreatedLogEntry<GraphSchema>(server.Schema);

            Assert.AreEqual(null, entry.Message);

            Assert.AreEqual(server.Schema.OperationTypes.Count, entry.SchemaSupportedOperationTypes.Count);
            foreach (var operation in server.Schema.OperationTypes)
                Assert.IsTrue(entry.SchemaSupportedOperationTypes.Contains(operation.Key.ToString().ToLower()));

            Assert.AreEqual(LogEventIds.SchemaInstanceCreated.Id, entry.EventId);
            Assert.AreEqual(GraphSchema.DEFAULT_NAME, entry.SchemaInstanceName);
            Assert.AreEqual(server.Schema.KnownTypes.Count, entry.SchemaGraphTypeCount);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.IsNotNull(entry.ToString());

            foreach (var type in server.Schema.KnownTypes)
            {
                var logGraphType = entry.GraphTypes
                    .Cast<SchemaGraphTypeLogItem>()
                    .SingleOrDefault(x => x.GraphTypeName == type.Name);

                Assert.IsNotNull(logGraphType);
                Assert.AreEqual(type.Name, logGraphType.GraphTypeName);
                Assert.AreEqual(type.Kind.ToString(), logGraphType.GraphTypeKind);
                Assert.AreEqual(type.Publish, logGraphType.IsPublished);

                var concreteType = server.Schema.KnownTypes.FindConcreteType(type);
                Assert.AreEqual(concreteType?.FriendlyName(true), logGraphType.GraphTypeType);

                int? totalFields = type is IGraphFieldContainer fc ? fc.Fields.Count : null;
                Assert.AreEqual(totalFields, logGraphType.GraphFieldCount);
            }
        }

        [Test]
        public void SchemaPipelineRegisteredLogEntry()
        {
            var server = new TestServerBuilder()
                .AddType<LogTestController>()
                .Build();

            var pipeline = server.ServiceProvider.GetService<ISchemaPipeline<GraphSchema, GraphFieldExecutionContext>>();
            var entry = new SchemaPipelineRegisteredLogEntry<GraphSchema>(pipeline);

            Assert.AreEqual(pipeline.MiddlewareComponentNames.Count, entry.MiddlewareCount);
            foreach (var item in pipeline.MiddlewareComponentNames)
                Assert.IsTrue(entry.MiddlewareComponents.Contains(item));

            Assert.AreEqual(LogEventIds.SchemaPipelineInstanceCreated.Id, entry.EventId);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(Constants.Pipelines.FIELD_EXECUTION_PIPELINE, entry.PipelineName);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void SchemaRouteRegisteredLogEntry()
        {
            var entry = new SchemaRouteRegisteredLogEntry<GraphSchema>("testRoute");

            Assert.AreEqual(LogEventIds.SchemaRouteRegistered.Id, entry.EventId);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual("testRoute", entry.SchemaRoutePath);
        }

        [Test]
        public void RequestReceivedLogEntry()
        {
            var serverBuilder = new TestServerBuilder()
                            .AddType<LogTestController>();
            serverBuilder.UserContext.Authenticate("fakeUserName");
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("{ testField }");
            var request = builder.OperationRequest;
            var context = builder.Build();

            var entry = new RequestReceivedLogEntry(context);

            Assert.AreEqual(LogEventIds.RequestReceived.Id, entry.EventId);
            Assert.AreEqual(request.Id, entry.OperationRequestId);
            Assert.AreEqual("fakeUserName", entry.Username);
            Assert.AreEqual(request.OperationName, entry.QueryOperationName);
            Assert.AreEqual("{ testField }", entry.QueryText);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void RequestCompletedLogEntry()
        {
            var server = new TestServerBuilder()
                            .AddType<LogTestController>()
                            .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("{ testField }");
            var request = builder.OperationRequest;
            var context = builder.Build();

            var mock = new Mock<IGraphOperationResult>();
            mock.Setup(x => x.Request).Returns(request);
            mock.Setup(x => x.Data).Returns(new ResponseFieldSet());
            mock.Setup(x => x.Messages).Returns(new GraphMessageCollection());

            context.Result = mock.Object;

            var entry = new RequestCompletedLogEntry(context);

            Assert.AreEqual(LogEventIds.RequestCompleted.Id, entry.EventId);
            Assert.AreEqual(request.Id, entry.OperationRequestId);
            Assert.AreEqual(false, entry.ResultHasErrors);
            Assert.AreEqual(true, entry.ResultHasData);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void QueryPlanCacheHitLogEntry()
        {
            var entry = new QueryPlanCacheHitLogEntry<GraphSchema>("abc123");

            Assert.AreEqual(LogEventIds.QueryCacheHit.Id, entry.EventId);
            Assert.AreEqual("abc123", entry.QueryPlanHashCode);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void QueryPlanCacheMissLogEntry()
        {
            var entry = new QueryPlanCacheMissLogEntry<GraphSchema>("abc123");

            Assert.AreEqual(LogEventIds.QueryCacheMiss.Id, entry.EventId);
            Assert.AreEqual("abc123", entry.QueryPlanHashCode);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public async Task QueryPlanCacheAddLogEntry()
        {
            var server = new TestServerBuilder()
                            .AddType<LogTestController>()
                            .Build();
            var queryPlan = await server.CreateQueryPlan("query Operation1{ field1 } query Operation2 { fieldException }");

            var entry = new QueryPlanCacheAddLogEntry("abc123", queryPlan);

            Assert.AreEqual(LogEventIds.QueryCacheAdd.Id, entry.EventId);
            Assert.AreEqual("abc123", entry.QueryPlanHashCode);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(queryPlan.Id, entry.QueryPlanId);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public async Task QueryPlanGeneratedLogEntry()
        {
            var server = new TestServerBuilder()
                            .AddType<LogTestController>()
                            .Build();
            var queryPlan = await server.CreateQueryPlan("query Operation1{ field1 } query Operation2 { fieldException }");

            var entry = new QueryPlanGeneratedLogEntry(queryPlan);

            Assert.AreEqual(LogEventIds.QueryPlanGenerationCompleted.Id, entry.EventId);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
            Assert.AreEqual(queryPlan.Id, entry.QueryPlanId);
            Assert.AreEqual(queryPlan.IsValid, entry.QueryPlanIsValid);
            Assert.AreEqual(queryPlan.Operations.Count, entry.QueryOperationCount);
            Assert.AreEqual(queryPlan.EstimatedComplexity, entry.QueryPlanEstimatedComplexity);
            Assert.AreEqual(queryPlan.MaxDepth, entry.QueryPlanMaxDepth);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void FieldResolutionStartedLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                             .AddType<LogTestController>()
                             .Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var resolutionContext = package.CreateResolutionContext();

            var entry = new FieldResolutionStartedLogEntry(resolutionContext);

            Assert.AreEqual(LogEventIds.FieldResolutionStarted.Id, entry.EventId);
            Assert.AreEqual(resolutionContext.Request.Id, entry.PipelineRequestId);
            Assert.AreEqual(resolutionContext.Request.Field.Mode.ToString(), entry.FieldExecutionMode);
            Assert.AreEqual(resolutionContext.Request.Field.Route.Path, entry.FieldPath);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void FieldResolutionCompletedLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                             .AddType<LogTestController>()
                             .Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var resolutionContext = package.CreateResolutionContext();
            var fieldRequest = package.FieldRequest;

            resolutionContext.Result = "15";

            var entry = new FieldResolutionCompletedLogEntry(resolutionContext);
            Assert.AreEqual(LogEventIds.FieldResolutionCompleted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(fieldRequest.Field.Route.Path, entry.FieldPath);
            Assert.AreEqual(fieldRequest.Field.TypeExpression.ToString(), entry.TypeExpression);
            Assert.AreEqual(true, entry.HasData);
            Assert.AreEqual(true, entry.ResultIsValid);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void FieldAuthorizationStartedLogEntry()
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                            .AddType<LogTestController>();

            builder.UserContext.Authenticate("bobSmith");
            var server = builder.Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var fieldRequest = package.FieldRequest;

            var authContext = package.CreateSecurityContext();
            authContext.AuthenticatedUser = server.SecurityContext.DefaultUser;

            var entry = new FieldAuthorizationStartedLogEntry(authContext);

            Assert.AreEqual(LogEventIds.FieldAuthorizationStarted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(fieldRequest.Field.Route.Path, entry.FieldPath);
            Assert.AreEqual(authContext.AuthenticatedUser?.RetrieveUsername(), entry.Username);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void FieldAuthorizationCompletedLogEntry()
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>();

            builder.UserContext.Authenticate("bobSmith");
            var server = builder.Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var fieldRequest = package.FieldRequest;
            var authContext = package.CreateSecurityContext();
            authContext.AuthenticatedUser = server.SecurityContext.DefaultUser;

            authContext.Result = FieldSecurityChallengeResult.Fail("test message 1");
            var entry = new FieldAuthorizationCompletedLogEntry(authContext);

            Assert.AreEqual(LogEventIds.FieldAuthorizationCompleted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(fieldRequest.Field.Route.Path, entry.FieldPath);
            Assert.AreEqual(authContext.AuthenticatedUser?.RetrieveUsername(), entry.Username);
            Assert.AreEqual(authContext.Result.Status.ToString(), entry.AuthorizationStatus);
            Assert.IsNotNull(entry.ToString());
            Assert.AreEqual(authContext.Result.LogMessage, entry.LogMessage);
        }

        [Test]
        public void FieldAuthenticationStartedLogEntry()
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                            .AddType<LogTestController>();

            builder.UserContext.Authenticate("bobSmith");
            var server = builder.Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var fieldRequest = package.FieldRequest;

            var authContext = package.CreateSecurityContext();
            authContext.AuthenticatedUser = server.SecurityContext.DefaultUser;

            var entry = new FieldAuthenticationStartedLogEntry(authContext);

            Assert.AreEqual(LogEventIds.FieldAuthenticationStarted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(fieldRequest.Field.Route.Path, entry.FieldPath);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void FieldAuthenticationCompletedLogEntry()
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>();
            builder.UserContext.Authenticate("bob-smith");
            var server = builder.Build();

            var ident = new Mock<IIdentity>();
            ident.Setup(x => x.Name).Returns("someOtherUser");
            var testUser = new ClaimsPrincipal();
            testUser.AddIdentity(new ClaimsIdentity(ident.Object));

            var authResult = new Mock<IAuthenticationResult>();
            authResult.Setup(x => x.User).Returns(testUser);
            authResult.Setup(x => x.AuthenticationScheme).Returns("testScheme");
            authResult.Setup(x => x.Suceeded).Returns(true);

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));

            var fieldRequest = package.FieldRequest;
            var authContext = package.CreateSecurityContext();
            authContext.AuthenticatedUser = server.SecurityContext.DefaultUser;
            authContext.Result = FieldSecurityChallengeResult.Fail("test message 1");

            var entry = new FieldAuthenticationCompletedLogEntry(authContext, authResult.Object);

            Assert.AreEqual(LogEventIds.FieldAuthenticationCompleted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(fieldRequest.Field.Route.Path, entry.FieldPath);
            Assert.AreEqual("someOtherUser", entry.Username); // ensure its the user from the authResult
            Assert.AreEqual("testScheme", entry.AuthenticationScheme);
            Assert.IsTrue(entry.AuthethenticationSuccess);
            Assert.IsNotNull(entry.ToString());
            Assert.IsNotNull(entry.LogMessage.ToString());
            Assert.AreEqual(authContext.Result.LogMessage, entry.LogMessage);
        }

        [Test]
        public void ActionMethodInvocationStartedLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>()
                                         .Build();
            var graphMethod = TemplateHelper.CreateActionMethodTemplate<LogTestController>(nameof(LogTestController.ExecuteField2)) as IGraphMethod;
            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var fieldRequest = package.FieldRequest;

            var entry = new ActionMethodInvocationStartedLogEntry(graphMethod, fieldRequest);

            Assert.AreEqual(LogEventIds.ControllerInvocationStarted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(graphMethod.Parent.InternalFullName, entry.ControllerName);
            Assert.AreEqual(graphMethod.Name, entry.ActionName);
            Assert.AreEqual(graphMethod.Route.Path, entry.FieldPath);
            Assert.AreEqual(graphMethod.ObjectType.ToString(), entry.SourceObjectType);
            Assert.AreEqual(graphMethod.IsAsyncField, entry.IsAsync);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void ActionMethodInvocationCompletedLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>()
                                         .Build();

            var graphMethod = TemplateHelper.CreateActionMethodTemplate<LogTestController>(nameof(LogTestController.ExecuteField2)) as IGraphMethod;
            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var resolutionContext = package.CreateResolutionContext();
            var fieldRequest = package.FieldRequest;

            var result = new object();

            var entry = new ActionMethodInvocationCompletedLogEntry(graphMethod, fieldRequest, result);

            Assert.AreEqual(LogEventIds.ControllerInvocationCompleted.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(graphMethod.Parent.InternalFullName, entry.ControllerName);
            Assert.AreEqual(graphMethod.Name, entry.ActionName);
            Assert.AreEqual(graphMethod.Route.Path, entry.FieldPath);
            Assert.AreEqual(result.GetType().FriendlyName(true), entry.ResultTypeName);
            Assert.IsNotNull(entry.ToString());
        }

        [Test]
        public void ActionMethodInvocationExceptionLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>()
                                         .Build();

            var graphMethod = TemplateHelper.CreateActionMethodTemplate<LogTestController>(nameof(LogTestController.ExecuteField2)) as IGraphMethod;
            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var fieldRequest = package.FieldRequest;

            var result = new object();

            var inner = new Exception("inner error");
            var exception = new TargetInvocationException("invocation error message", inner);
            var entry = new ActionMethodInvocationExceptionLogEntry(graphMethod, fieldRequest, exception);

            Assert.AreEqual(LogEventIds.ControllerInvocationException.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(graphMethod.Parent.InternalFullName, entry.ControllerTypeName);
            Assert.AreEqual(graphMethod.Name, entry.ActionName);
            Assert.IsNotNull(entry.ToString());

            var exceptionEntry = entry.Exception as ExceptionLogItem;
            Assert.IsNotNull(exceptionEntry);
            Assert.AreEqual(exception.Message, exceptionEntry.ExceptionMessage);
            Assert.AreEqual(exception.StackTrace, exceptionEntry.StackTrace);
            Assert.AreEqual(exception.GetType().FriendlyName(true), exceptionEntry.TypeName);
        }

        [Test]
        public void ActionMethodUnhandledExceptionLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                                         .AddType<LogTestController>()
                                         .Build();

            var package = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ExecuteField2));
            var graphMethod = TemplateHelper.CreateActionMethodTemplate<LogTestController>(nameof(LogTestController.ExecuteField2)) as IGraphMethod;
            var fieldRequest = package.FieldRequest;

            var result = new object();

            var exception = new Exception("inner error");
            var entry = new ActionMethodUnhandledExceptionLogEntry(graphMethod, fieldRequest, exception);

            Assert.AreEqual(LogEventIds.ControllerUnhandledException.Id, entry.EventId);
            Assert.AreEqual(fieldRequest.Id, entry.PipelineRequestId);
            Assert.AreEqual(graphMethod.Parent.InternalFullName, entry.ControllerTypeName);
            Assert.AreEqual(graphMethod.Name, entry.ActionName);
            Assert.IsNotNull(entry.ToString());

            var exceptionEntry = entry.Exception as ExceptionLogItem;
            Assert.IsNotNull(exceptionEntry);
            Assert.AreEqual(exception.Message, exceptionEntry.ExceptionMessage);
            Assert.AreEqual(exception.StackTrace, exceptionEntry.StackTrace);
            Assert.AreEqual(exception.GetType().FriendlyName(true), exceptionEntry.TypeName);
        }

        [Test]
        public void TypeSystemDirectiveAppliedLogEntry()
        {
            var directive = new Mock<IDirective>();
            directive.Setup(x => x.Name).Returns("The Directive");
            directive.Setup(x => x.InternalName).Returns("The Directive Internal");

            var item = new Mock<ISchemaItem>();
            item.Setup(x => x.Route).Returns(new AspNet.Schemas.Structural.GraphFieldPath(GraphCollection.Types, "path1"));

            var exception = new Exception("inner error");
            var entry = new TypeSystemDirectiveAppliedLogEntry<GraphSchema>(directive.Object, item.Object);

            Assert.AreEqual(LogEventIds.TypeSystemDirectiveApplied.Id, entry.EventId);
            Assert.AreEqual(directive.Object.Name, entry.DirectiveName);
            Assert.AreEqual(directive.Object.InternalName, entry.DirectiveInternalName);
            Assert.AreEqual(item.Object.Route.Path, entry.SchemaItemPath);
            Assert.AreEqual(typeof(GraphSchema).FriendlyName(true), entry.SchemaTypeName);
        }
    }
}