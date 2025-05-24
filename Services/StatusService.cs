using System.Reflection;
using NxSuite.Interfaces;

namespace PROJECT_TEMPLATE.Services;

public class StatusService(IMqService mqService)
{
    public async Task<dynamic?> GetStatus(WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("GET")))
            .Select(e => e.RoutePattern.RawText)
            .ToList();
        var mqTest = await mqService.TestConnectionAsync();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        return new
        {
            Description = "NxSuite SignUp",
            Version = version,
            Status = "OK",
            DateTime = DateTime.Now,
            MessageQueueConnected = mqTest,
            EndPoints = endpoints,
        };
    }
}