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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.JMeterAPI.Model;
    using Microsoft.EntityFrameworkCore;

    [GraphRoute("organizations")]
    public class OrganizationsController : GraphController
    {
        private BakeryContext _context;

        public OrganizationsController(BakeryContext context)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        [QueryRoot("organization", typeof(Organization))]
        public Task<IGraphActionResult> RootRetrieveOrganization(int id)
            => this.RetrieveOrg(id);

        [Query("find", typeof(Organization))]
        public Task<IGraphActionResult> RetrieveOrganization(int id)
            => this.RetrieveOrg(id);

        [Query("search", typeof(IEnumerable<Organization>))]
        public async Task<IGraphActionResult> RetrieveOrganization(string nameLike)
        {
            var orgs = await this.AddSubEntities(_context.Organizations)
                .Where(x => x.Name.StartsWith(nameLike))
                .ToListAsync();

            return this.Ok(orgs);
        }

        private async Task<IGraphActionResult> RetrieveOrg(int id)
        {
            var org = await this.AddSubEntities(_context.Organizations).SingleOrDefaultAsync(x => x.Id == id);
            return this.Ok(org);
        }

        private IQueryable<Organization> AddSubEntities(IQueryable<Organization> query)
        {
            return query
                .Include(x => x.Bakeries)
                .Include(x => x.Recipes)
                .Include(x => x.Invoices)
                .ThenInclude(x => x.LineItems);
        }
    }
}