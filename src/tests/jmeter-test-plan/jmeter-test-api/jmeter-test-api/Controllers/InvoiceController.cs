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

    [GraphRoute("invoices")]
    public class InvoiceController : GraphController
    {
        private BakeryContext _context;

        public InvoiceController(BakeryContext context)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        [QueryRoot("invoice", typeof(Invoice))]
        public Task<IGraphActionResult> RootRetrieveInvoice(int id)
            => this.RetrieveInvoiceById(id);

        [Query("find", typeof(Invoice))]
        public Task<IGraphActionResult> RetrieveInvoice(int id)
            => this.RetrieveInvoiceById(id);

        [Query("search", typeof(IEnumerable<Invoice>))]
        public async Task<IGraphActionResult> RetrieveInvoice(
            int? bakeryId = null,
            int? organizationId = null,
            string customerName = null,
            decimal? minTotalCost = null,
            decimal? maxTotalCost = null)
        {
            var query = this.AddSubEntities(_context.Invoices);

            query = query
              .Include(x => x.LineItems)
              .ThenInclude(x => x.Pastry)
              .Include(x => x.Organization)
              .Include(x => x.Bakery);

            if (bakeryId.HasValue)
                query = query.Where(x => x.BakeryId == bakeryId.Value);

            if (organizationId.HasValue)
                query = query.Where(x => x.OrganizationId == organizationId.Value);

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                customerName = customerName.Trim();
                query = query.Where(x => x.CustomerName.StartsWith(customerName));
            }

            if (minTotalCost.HasValue)
                query = query.Where(x => x.TotalCost >= minTotalCost.Value);

            if (maxTotalCost.HasValue)
                query = query.Where(x => x.TotalCost <= maxTotalCost.Value);

            var invoices = await query.ToListAsync();

            return this.Ok(invoices);
        }

        private async Task<IGraphActionResult> RetrieveInvoiceById(int id)
        {
            var invoice = await this.AddSubEntities(_context.Invoices)
                .SingleOrDefaultAsync(x => x.Id == id);
            return this.Ok(invoice);
        }

        private IQueryable<Invoice> AddSubEntities(IQueryable<Invoice> query)
        {
            return query
                .Include(x => x.LineItems)
                .Include(x => x.Organization)
                .Include(x => x.Bakery);
        }
    }
}