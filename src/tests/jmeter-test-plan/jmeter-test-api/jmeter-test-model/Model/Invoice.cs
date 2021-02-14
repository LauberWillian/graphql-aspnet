// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// An invoice created as a result of a sale of items by a bakery.
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public int BakeryId { get; set; }

        [Required]
        [StringLength(250)]
        public string CustomerName { get; set; }

        public decimal TotalCost { get; set; }

        [ForeignKey(nameof(BakeryId))]
        public Bakery Bakery { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        public ICollection<InvoiceLineItem> LineItems { get; set; }
    }
}