using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using RpiApi.Repository.v1_0;

using System.Reflection;

using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RpiApi.Swagger.Filter;


namespace RpiApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
    }
    public static IConfiguration Configuration { get; private set; }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();

        #region Services
        services.AddTransient<ISensorsDa, SensorsDa>();
        #endregion

        services.AddSingleton<IConfiguration>(Configuration);

        services.AddApiVersioning(options =>
                                    {
                                        options.ReportApiVersions = true;
                                        options.AssumeDefaultVersionWhenUnspecified = true;
                                        options.DefaultApiVersion = new ApiVersion(1,0);
                                        options.ApiVersionReader = new UrlSegmentApiVersionReader();
                                    })
                                    .AddApiExplorer(options =>
                                            {
                                                options.GroupNameFormat = "'v'VVVV";
                                                options.SubstituteApiVersionInUrl = false;
                                            });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Raspberry API", Version = "v1.0" });
            c.TagActionsBy(api =>
                                {
                                    if (api.GroupName != null)
                                    {
                                        return new[] { api.GroupName };
                                    }

                                    if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                                    {
                                        return new[] { controllerActionDescriptor.ControllerName };
                                    }

                                    throw new InvalidOperationException("Unable to determine tag for endpoint.");
                                });
            c.DocInclusionPredicate((docName, apiDesc) =>
                    {
                        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                            return false;

                        IEnumerable<ApiVersion> versions = methodInfo.DeclaringType
                                                                    .GetCustomAttributes(true)
                                                                    .OfType<ApiVersionAttribute>()
                                                                    .SelectMany(attr => attr.Versions);

                        return versions.Any(v => $"v{v.ToString()}" == docName);
                    });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.CustomSchemaIds(schemaId => schemaId.FullName);
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            c.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
            c.DocumentFilter<RemoveDefaultApiVersionRouteDocumentFilter>();
            //c.OperationFilter<AuthorizeOperationFilter>();
            c.OperationFilter<RemoveVersionParameterFilter>();
            c.SchemaFilter<NamespaceSchemaFilter>();
            c.EnableAnnotations();
        });
    }
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        
        app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()/*.WithOrigins("https://localhost:4200")*/
                .AllowAnyOrigin()
        );
        app.UsePathBase("/api");
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
            RequestPath = new PathString("/Resources")
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSwagger(options =>
                                   options.RouteTemplate = "swagger/{documentName}/swagger.json");
        app.UseSwaggerUI(c =>
                             {
                                 foreach (ApiVersionDescription description in provider.ApiVersionDescriptions.Reverse())
                                 {
                                     c.SwaggerEndpoint($"{description.GroupName}/swagger.json", $"LiTOs {description.GroupName}");
                                 }
                                 c.RoutePrefix = "swagger";
                                 c.InjectStylesheet("custom.css");
                                 c.DocumentTitle = "Raspberry API swagger";
                                 //c.DocExpansion(DocExpansion.None);
                             });
    }
}