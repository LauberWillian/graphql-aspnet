namespace GraphQL.AspNet.Tests.Internal.Templating.DocFxTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ClassWithGenericMethod
    {
        /// <summary>
        /// Generic Method Summary is here.
        /// </summary>
        /// <typeparam name="T">The input type</typeparam>
        /// <param name="firstParam">The first parameter.</param>
        /// <param name="secondParam">The second parameter.</param>
        /// <returns>System.String.</returns>
        public string GenericMethod<T>(string firstParam, T secondParam)
        {
            return null;
        }
    }
}
