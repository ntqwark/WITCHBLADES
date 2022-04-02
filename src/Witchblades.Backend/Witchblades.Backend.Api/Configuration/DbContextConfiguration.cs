﻿using Microsoft.EntityFrameworkCore;
using Witchblades.Backend.Data;

namespace Witchblades.Backend.Api.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static void ConfigureSqlDbContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("WitchbladesContext");

            services.AddDbContext<WitchbladesContext>(
                options => options.UseSqlServer(connectionString));
        }

        public static void ConfigureDbContextForTests(
            this IServiceCollection services)
        {
            services.AddDbContext<WitchbladesContext>(
                options => options.UseInMemoryDatabase("WitchbladesContext.Tests.InMemory"));
        }
    }
}
