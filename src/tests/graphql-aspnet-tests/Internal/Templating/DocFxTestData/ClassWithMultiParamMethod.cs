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
    public class ClassWithMultiParamMethod
    {
        /// <summary>
        /// This is a method with three parameters.
        /// </summary>
        /// <param name="firstParam">This is the 1st parameter called firstParam.</param>
        /// <param name="secondParam">The 2nd parameter called second param.</param>
        /// <param name="thirdParam">The third parameter called third param.</param>
        /// <returns>System.String.</returns>
        public int MultiParamMethod(string firstParam, int secondParam, ChildClassWithInheritDoc thirdParam)
        {
            return 0;
        }
    }
}