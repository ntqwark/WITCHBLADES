﻿using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Witchblades.Backend.Api.Configuration.ServiceCollectionConfiguration
{
    public static class DevelopmentTimeFeaturesExtensions
    {
        /// <summary>
        /// Add swagger UI
        /// </summary>
        /// <param name="app"></param>
        public static void UseDevelopmentTimeFeatures(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var apiProvider = scope.ServiceProvider.GetService<IApiVersionDescriptionProvider>();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in apiProvider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });
            }
        }
    }
}