using MCP.Demo.Web.Startup;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();


//builder.Services.AddOpenTelemetry()
//    .WithTracing(b => b.AddSource("*").AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
//    .WithMetrics(b => b.AddMeter("*").AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
//    .WithLogging()
//    .UseOtlpExporter();

builder.Services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
    //client.DefaultRequestHeaders.UserAgent.Add(new )
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapMcp();

app.Run();
