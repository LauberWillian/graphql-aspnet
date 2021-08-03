namespace GraphQL.AspNet.Tests.Internal.Templating.DocFxTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ClassWith2ParamGenericMethod
    {
        /// <summary>
        /// Generic Method Summary for two params is here.
        /// </summary>
        /// <typeparam name="T">The t input type</typeparam>
        /// <typeparam name="K">The k input type</typeparam>
        /// <param name="firstParam">The first parameter.</param>
        /// <param name="secondParamSecondGeneric">The second parameter second generic.</param>
        /// <param name="thirdParamFirstGeneric">The third parameter first generic.</param>
        /// <returns>System.String.</returns>
        public string GenericMethod2<T, K>(string firstParam, K secondParamSecondGeneric, T thirdParamFirstGeneric)
        {
            return null;
        }
    }
}
