using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NxSuite.Api;
using NxSuite.Api.Middlewares;
using NxSuite.Api.Shared;
using NxSuite.Interfaces;
using PROJECT_TEMPLATE.Database;
using PROJECT_TEMPLATE.Examples;
using PROJECT_TEMPLATE.Models;
using PROJECT_TEMPLATE.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var nxSuiteDataConnectionString = builder.Configuration.GetConnectionString("NxDb");
builder.Services.AddSingleton<INxConnectionFactory>(new NxSqlConnectionFactory(nxSuiteDataConnectionString));

builder.Services
    .AddDbContext<NxDbContext>(options =>
    {
        options.UseSqlServer(nxSuiteDataConnectionString)
            .ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
            );
    });

builder.NxSwagger();
var settings = NxApiBuilder.GetSettings(nxSuiteDataConnectionString);
builder.NxSystemSettings();
builder.NxCorsPolicy(settings);
builder.NxAddMqService(settings);
builder.NxSerilog();
builder.NxRegisterServices<StatusService>(typeof(StatusService).Namespace);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignUp API V1"); });
}

// app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", async (StatusService s) => await s.GetStatus(app)).WithOpenApi();
app.MapGet("/try-for-free", async (TryForFreeService s) => await s.GetAsync()).WithOpenApi();
app.MapPost("/try-for-free", async (TryForFreeService s, TryForFreeModel request) =>
{
    try
    {
        await s.AddAsync(request);
        return Results.Ok(new {status = true});
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error occurred while processing the signup request: {@Request}", request);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithSwaggerExample<TryForFreeModel>(ExampleJson.TryForFreeExample());
app.MapPost("/verify-invitation", async (VerifyInvitationRequest request, VerifyService verifyService) =>
{
    if (request.InvitationId == Guid.Empty)
    {
        return Results.BadRequest("Invitation ID cannot be empty.");
    }
    try
    {
        var result = await verifyService.VerifyInvitationAsync(request.InvitationId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ApplicationException ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/invitation/{id:guid}", async (Guid id, VerifyService service) =>
{
    var result = await service.GetInvitationDetailsAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.InitializeMqService();

app.Run();