// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.JMeterAPI.Model;
    using Microsoft.EntityFrameworkCore;

    [GraphRoute("bakeries")]
    public class BakeryController : GraphController
    {
        private BakeryContext _context;

        public BakeryController(BakeryContext context)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        [QueryRoot("bakery", typeof(Bakery))]
        public Task<IGraphActionResult> RootRetrieveBakery(int id)
            => this.RetrieveBakeryInternal(id);

        [Query("find", typeof(Bakery))]
        public Task<IGraphActionResult> RetrieveBakery(int id)
            => this.RetrieveBakeryInternal(id);

        [Query("search", typeof(IEnumerable<Bakery>))]
        public async Task<IGraphActionResult> RetrievePasteryRecipe(string nameLike = null, string addressLike = null, int? orgId = null)
        {
            var query = this.AddSubEntities(_context.Bakeries);

            if (!string.IsNullOrWhiteSpace(addressLike))
            {
                addressLike = addressLike.Trim();
                query = query.Where(x => x.Address1.Contains(addressLike) ||
                    x.Address2.Contains(addressLike) ||
                    x.City.Contains(addressLike) ||
                    x.State.Contains(addressLike) ||
                    x.PostalCode.Contains(addressLike));
            }

            if (!string.IsNullOrWhiteSpace(nameLike))
            {
                nameLike = nameLike.Trim();
                query = query.Where(x => x.Name.Contains(nameLike));
            }

            if (orgId.HasValue)
            {
                query = query.Where(x => x.OrganizationId == orgId.Value);
            }

            var bakeries = await query.ToListAsync();

            return this.Ok(bakeries);
        }

        private async Task<IGraphActionResult> RetrieveBakeryInternal(int id)
        {
            var bakery = await this.AddSubEntities(_context.Bakeries).SingleOrDefaultAsync(x => x.Id == id);
            return this.Ok(bakery);
        }

        private IQueryable<Bakery> AddSubEntities(IQueryable<Bakery> query)
        {
            return query
                .Include(x => x.Organization)
                .Include(x => x.Stock)
                .Include(x => x.Invoices)
                .ThenInclude(x => x.LineItems);
        }
    }
}