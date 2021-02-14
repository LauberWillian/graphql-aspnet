namespace GraphQL.AspNet.JMeterAPI
{
    using System.Security.Cryptography.X509Certificates;
    using GraphQL.AspNet.JMeterAPI.Model;
    using Microsoft.EntityFrameworkCore;

    public class BakeryContext : DbContext
    {
        public BakeryContext(DbContextOptions<BakeryContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PastryStock>()
                .Property(x => x.SalePriceEach)
                .HasPrecision(15, 2);

            modelBuilder.Entity<Invoice>()
                .Property(x => x.TotalCost)
                .HasPrecision(15, 2);

            modelBuilder.Entity<InvoiceLineItem>()
                .Property(x => x.SalePriceEach)
                .HasPrecision(15, 2);
        }

        public DbSet<Bakery> Bakeries { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
    }
}