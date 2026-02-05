// Service configuration extensions for ASP.NET Core application
// Provides helper methods for configuring OpenTelemetry, health checks, and service defaults

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MCP.Demo.Web.Startup
{
    /// <summary>
    /// Extension methods for configuring application services and defaults
    /// </summary>
    public static class ServiceExtensions
    {
        // Health check endpoint paths
        public const string HealthEndpointPath = "/health";
        public const string AlivenessEndpointPath = "/alive";

        /// <summary>
        /// Add default services including OpenTelemetry, health checks, and service discovery
        /// </summary>
        /// <typeparam name="TBuilder">The host application builder type</typeparam>
        /// <param name="builder">The host application builder instance</param>
        /// <returns>The modified builder for method chaining</returns>
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            // Configure OpenTelemetry for observability
            builder.ConfigureOpenTelemetry();

            // Add health check endpoints
            builder.AddDefaultHealthChecks();

            // Enable service discovery for service-to-service communication
            builder.Services.AddServiceDiscovery();

            // Configure HTTP client defaults with resilience and service discovery
            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });

            return builder;
        }

        /// <summary>
        /// Configure OpenTelemetry tracing, metrics, and logging
        /// </summary>
        /// <typeparam name="TBuilder">The host application builder type</typeparam>
        /// <param name="builder">The host application builder instance</param>
        /// <returns>The modified builder for method chaining</returns>
        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder: IHostApplicationBuilder
        {
            // Configure logging with OpenTelemetry instrumentation
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            // Configure metrics and tracing
            builder.Services.AddOpenTelemetry()
                // Add metrics collection from ASP.NET Core, HTTP clients, and runtime
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                // Add distributed tracing
                .WithTracing(tracing =>
                {
                    // Trace requests from this application
                    tracing.AddSource(builder.Environment.ApplicationName)
                        // Instrument ASP.NET Core with filtering for health check endpoints
                        .AddAspNetCoreInstrumentation(tracing =>
                            tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                        )
                        // Instrument HTTP client calls
                        .AddHttpClientInstrumentation();
                });

            // Add exporters for telemetry data
            builder.AddOpenTelemetryExporters();

            return builder;
        }

        /// <summary>
        /// Configure OpenTelemetry exporters based on environment configuration
        /// </summary>
        /// <typeparam name="TBuilder">The host application builder type</typeparam>
        /// <param name="builder">The host application builder instance</param>
        /// <returns>The modified builder for method chaining</returns>
        public static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder: IHostApplicationBuilder
        {
            // Check if OTLP exporter is configured
            var useOltpExporter = !string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOltpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            // Check if Azure Application Insights is configured
            if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitor();
            }

            return builder;
        }

        /// <summary>
        /// Add default health check endpoints
        /// </summary>
        /// <typeparam name="TBuilder">The host application builder type</typeparam>
        /// <param name="builder">The host application builder instance</param>
        /// <returns>The modified builder for method chaining</returns>
        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder: IHostApplicationBuilder
        {
            // Register a basic health check for application liveness
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }

        /// <summary>
        /// Map default health check endpoints to the application
        /// </summary>
        /// <param name="app">The web application instance</param>
        /// <returns>The modified application for method chaining</returns>
        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            // Only map health endpoints in development environment
            if (app.Environment.IsDevelopment())
            {
                // General health check endpoint
                app.MapHealthChecks(HealthEndpointPath);
                
                // Liveness probe endpoint - used for Kubernetes/orchestrator readiness
                app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live")
                });
            }

            return app;
        }
    }
}
