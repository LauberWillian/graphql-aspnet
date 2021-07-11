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

    public class PastryController : GraphController
    {
        private BakeryContext _context;

        public PastryController(BakeryContext context)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        [QueryRoot("recipe", typeof(PastryRecipe))]
        public Task<IGraphActionResult> RootRetrievePastryRecipe(int id)
            => this.RetrieveRecipe(id);

        [Query("find", typeof(PastryRecipe))]
        public Task<IGraphActionResult> RetrievePastryRecipe(int id)
            => this.RetrieveRecipe(id);

        [Query("search", typeof(IEnumerable<PastryRecipe>))]
        public async Task<IGraphActionResult> RetrievePastryRecipe(string nameLike = null, string bodyContains = null)
        {
            var query = this.AddSubEntities(_context.Recipes);
            if (!string.IsNullOrWhiteSpace(nameLike))
            {
                nameLike = nameLike.Trim();
                query = query.Where(x => x.Name.StartsWith(nameLike));
            }

            if (!string.IsNullOrWhiteSpace(bodyContains))
            {
                bodyContains = bodyContains.Trim();
                query = query.Where(x => x.RecipeText.Contains(bodyContains, System.StringComparison.OrdinalIgnoreCase));
            }

            var recipes = await query.ToListAsync();

            return this.Ok(recipes);
        }

        private async Task<IGraphActionResult> RetrieveRecipe(int id)
        {
            var org = await this.AddSubEntities(_context.Recipes).SingleOrDefaultAsync(x => x.Id == id);
            return this.Ok(org);
        }

        private IQueryable<PastryRecipe> AddSubEntities(IQueryable<PastryRecipe> query)
        {
            return query
                .Include(x => x.Organization);
        }
    }
}