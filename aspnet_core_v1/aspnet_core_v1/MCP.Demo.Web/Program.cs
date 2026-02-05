// MCP (Model Context Protocol) Demo Web API Server
// This ASP.NET Core application serves as an MCP server that provides tools
// accessible via HTTP transport to MCP clients

using MCP.Demo.Web.Startup;

// Create web application builder with default settings
var builder = WebApplication.CreateBuilder(args);

// Configure CORS policy to allow requests from any origin
// This is useful for development/demo purposes - should be restricted in production
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowall", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

// Add service defaults including OpenTelemetry, health checks, and service discovery
builder.AddServiceDefaults();

// Configure MCP Server with HTTP transport and auto-register tools from the assembly
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Note: OpenTelemetry configuration commented out - can be enabled for monitoring
//builder.Services.AddOpenTelemetry()
//    .WithTracing(b => b.AddSource("*").AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
//    .WithMetrics(b => b.AddMeter("*").AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
//    .WithLogging()
//    .UseOtlpExporter();

// Configure HTTP client factory with a named client for external API calls
builder.Services.AddHttpClient("MyApi", client =>
{
    // Target external API running on localhost:8081
    client.BaseAddress = new Uri("http://localhost:8081");
    //client.DefaultRequestHeaders.UserAgent.Add(new )
});

// Build the application
var app = builder.Build();

// Apply CORS policy to all requests
app.UseCors("allowall");

// Map default endpoints (health checks, service discovery, etc.)
app.MapDefaultEndpoints();

// Map MCP server endpoints
app.MapMcp();

// Run the application
app.Run();
