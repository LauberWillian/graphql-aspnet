// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A single bakery store owned by an organization.
    /// </summary>
    public class Bakery
    {
        [Key]
        public int Id { get; set; }

        [GraphSkip]
        public int OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [Required]
        [StringLength(75)]
        public string Name { get; set; }

        [StringLength(150)]
        public string Address1 { get; set; }

        [StringLength(150)]
        public string Address2 { get; set; }

        [StringLength(150)]
        public string City { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(15)]
        public string PostalCode { get; set; }

        public ICollection<PastryStock> Stock { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }
}