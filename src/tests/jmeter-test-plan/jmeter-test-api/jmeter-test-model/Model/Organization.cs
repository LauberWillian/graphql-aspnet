// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A top level company that owns N number of bakeries.
    /// </summary>
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(75)]
        public string Name { get; set; }

        [StringLength(150)]
        public string Address1 { get; set; }

        [StringLength(150)]
        public string Address2 { get; set; }

        [StringLength(150)]
        public string City { get; set; }

        [StringLength(150)]
        public string State { get; set; }

        [StringLength(150)]
        public string PostalCode { get; set; }

        public ICollection<Bakery> Bakeries { get; set; }

        public ICollection<PastryRecipe> Recipes { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }
}