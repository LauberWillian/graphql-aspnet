// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// The number of items of a given recipe a bakery has for sale
    /// at some time.
    /// </summary>
    public class PastryStock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [GraphSkip]
        public int BakeryId { get; set; }

        [Required]
        [GraphSkip]
        public int PastryRecipeId { get; set; }

        [Required]
        public int NumberForSale { get; set; }

        [Required]
        public decimal SalePriceEach { get; set; }

        [ForeignKey(nameof(BakeryId))]
        public Bakery Bakery { get; set; }

        [ForeignKey(nameof(PastryRecipeId))]
        public PastryRecipe Recipe { get; set; }
    }
}