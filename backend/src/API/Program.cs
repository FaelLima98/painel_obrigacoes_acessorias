using EAuditoria.API.Endpoints;
using EAuditoria.API.Middleware;
using EAuditoria.Application;
using EAuditoria.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string CorsPolicy = "frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Tratamento global de exceções (ProblemDetails RFC 7807)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);

app.MapHealthEndpoints();
app.MapCompaniesEndpoints();
app.MapObligationsEndpoints();
app.MapDeliveriesEndpoints();
app.MapAlertsEndpoints();
app.MapDashboardEndpoints();

app.Run();

// Expõe a classe Program para o WebApplicationFactory nos testes de integração.
public partial class Program;
