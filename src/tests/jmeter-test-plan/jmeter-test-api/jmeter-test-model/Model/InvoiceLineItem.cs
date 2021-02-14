// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class InvoiceLineItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NumberSold { get; set; }

        [Required]
        public decimal SalePriceEach { get; set; }

        public decimal LineItemTotal => NumberSold * SalePriceEach;

        [Required]
        public int InvoiceId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        [Required]
        public int PastryId { get; set; }

        [ForeignKey(nameof(PastryId))]
        public PastryRecipe Pastry { get; set; }
    }
}