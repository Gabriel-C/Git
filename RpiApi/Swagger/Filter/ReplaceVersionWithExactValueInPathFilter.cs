using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RpiApi.Swagger.Filter;

public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = new OpenApiPaths();
        foreach ((string key, OpenApiPathItem value) in swaggerDoc.Paths)
        {
            paths.Add(key.Replace("v{version}", swaggerDoc.Info.Version), value);
        }
        swaggerDoc.Paths = paths;
    }
}