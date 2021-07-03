// *************************************************************
// project:  GraphQL JMeter API
// *************************************************************

namespace GraphQL.AspNet.JMeterAPI
{
    using System.IO;
    using GraphQL.AspNet.Configuration.Mvc;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="env">The environment settings.</param>
        /// <param name="configuration">The configuration.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.Environment = env;
            this.Configuration = configuration;

            var dir = new DirectoryInfo(this.DataDirectory);
            if (!dir.Exists)
                dir.Create();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging();
            services.AddSingleton<BakeryContextDataSeeder>();

            services.AddDbContext<BakeryContext>((builder) =>
            {
                var cnnString = Configuration.GetConnectionString("BakeryDb");
                cnnString = cnnString.Replace("|DataDirectory|", this.DataDirectory);
                builder.UseSqlServer(cnnString);
            });

            services.AddGraphQL(options =>
            {
                options.AddGraphAssembly(typeof(BakeryContext).Assembly);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, BakeryContext context)
        {
            var logger = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger<Startup>();

            // Seed the test database if needed
            if (File.Exists(Path.Combine(this.DataDirectory, "bakery-jmeter-api.mdf")))
            {
                logger.LogInformation("Database exists, skipping seed data.");
            }
            else
            {
                logger.LogInformation("Creating DB ...");
                logger.LogInformation("Generating Database Schema ...");
                context.Database.Migrate();

                logger.LogInformation("Loading Seed Data ...");

                var seeder = app.ApplicationServices.GetService<BakeryContextDataSeeder>();
                seeder.Seed(context);
                logger.LogInformation("Database Seeding Complete.");
            }

            app.UseAuthorization();

            app.UseGraphQL();
        }

        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Gets the configuration used for this application.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; }

        private string DataDirectory => Path.Combine(this.Environment.ContentRootPath, "App_Data");

        public bool DbExists { get; private set; }
    }
}