// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A propritery recipe for a given Pastry. Owned by an organization and sold
    /// by its bakeries.
    /// </summary>
    public class PastryRecipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(1500)]
        public string RecipeText { get; set; }

        [GraphSkip]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }
    }
}