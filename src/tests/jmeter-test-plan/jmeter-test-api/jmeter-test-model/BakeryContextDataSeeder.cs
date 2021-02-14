// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Bogus;
    using GraphQL.AspNet.JMeterAPI.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class BakeryContextDataSeeder
    {
        private const int TOTAL_ORGS = 100;
        private const int RECIPES_PER_ORG = 50;
        private const int BAKERIES_PER_ORG = 100;
        private const int STOCK_PER_RECIPE = int.MaxValue;

        private int _orgId = 0;
        private int _recipeId = 0;
        private int _bakeryId = 0;
        private int _stockId = 0;

        private ILogger<BakeryContextDataSeeder> _logger;

        public BakeryContextDataSeeder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BakeryContextDataSeeder>();
        }

        public void Seed(BakeryContext context)
        {
            var orgs = this.CreateOrganizations(TOTAL_ORGS);

            _logger.LogInformation($"Staging Seed Data");
            foreach (var org in orgs)
            {
                context.Add(org);
            }

            var expectedCount = TOTAL_ORGS + // orgs
                                (TOTAL_ORGS * RECIPES_PER_ORG) + // recipes
                                (TOTAL_ORGS * BAKERIES_PER_ORG) + // bakeries
                                (TOTAL_ORGS * BAKERIES_PER_ORG * RECIPES_PER_ORG); // stock per bakery

            _logger.LogInformation($"Updating Database (approx. {expectedCount}  records)...");
            var records = context.SaveChanges();
            _logger.LogInformation($"{records} records created");
        }

        private List<Organization> CreateOrganizations(int count)
        {
            var faker = new Faker<Organization>()
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address1, f => f.Address.StreetAddress())
                .RuleFor(x => x.Address2, f => f.Address.SecondaryAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.State, f => f.Address.State())
                .RuleFor(x => x.PostalCode, f => f.Address.ZipCode());

            _logger.LogInformation($"Creating {TOTAL_ORGS} organizations");
            var list = faker.Generate(TOTAL_ORGS);
            foreach (var org in list)
            {
                this.AddRecipes(org);
                this.AddBakeries(org);
            }

            return list;
        }

        private void AddRecipes(Organization org)
        {
            var faker = new Faker<PastryRecipe>()
                .RuleFor(x => x.Name, f => f.Commerce.Product())
                .RuleFor(x => x.RecipeText, f => f.Lorem.Paragraphs(4))
                .RuleFor(x => x.OrganizationId, org.Id)
                .RuleFor(x => x.Organization, org);

            _logger.LogInformation($"Seeding {RECIPES_PER_ORG} recipes for {org.Name}");
            var recipes = faker.Generate(RECIPES_PER_ORG);
            org.Recipes = recipes;
        }

        private void AddBakeries(Organization org)
        {
            var faker = new Faker<Bakery>()
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address1, f => f.Address.StreetAddress())
                .RuleFor(x => x.Address2, f => f.Address.SecondaryAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.State, f => f.Address.State())
                .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
                .RuleFor(x => x.OrganizationId, org.Id)
                .RuleFor(x => x.Organization, org);

            _logger.LogInformation($"Seeding {BAKERIES_PER_ORG} bakeries for {org.Name}");
            var bakeries = faker.Generate(BAKERIES_PER_ORG);
            org.Bakeries = bakeries;

            foreach (var bakery in bakeries)
                this.LoadInventory(org, bakery);
        }

        private void LoadInventory(Organization org, Bakery bakery)
        {
            var i = 0;
            var faker = new Faker<PastryStock>()
                .RuleFor(x => x.BakeryId, bakery.Id)
                .RuleFor(x => x.Bakery, bakery)
                .RuleFor(x => x.PastryRecipeId, org.Recipes.ElementAt(i).Id)
                .RuleFor(x => x.Recipe, org.Recipes.ElementAt(i))
                .RuleFor(x => x.NumberForSale, STOCK_PER_RECIPE)
                .RuleFor(x => x.SalePriceEach, f => Convert.ToDecimal(f.Commerce.Price(0, 5, 2)));

            _logger.LogInformation($"Seeding {org.Recipes.Count} stock items for bakery {bakery.Name}");
            var allStock = new List<PastryStock>();
            for (i = 0; i < org.Recipes.Count; i++)
            {
                var stock = faker.Generate();
                allStock.Add(stock);
            }

            bakery.Stock = allStock;
        }
    }
}