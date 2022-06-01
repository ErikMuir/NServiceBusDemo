using Demo.Workflow.Messages;
using NServiceBus;

Console.Title = "Api";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" }));

builder.Host.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("Demo.Api");

    endpointConfiguration.SendOnly();

    var transport = endpointConfiguration.UseTransport<LearningTransport>();

    transport.Routing().RouteToEndpoint(
        assembly: typeof(BeginWorkflow).Assembly,
        destination: "Demo.Workflow");

    return endpointConfiguration;
});

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(c => c.MapControllers());
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

app.Run();
