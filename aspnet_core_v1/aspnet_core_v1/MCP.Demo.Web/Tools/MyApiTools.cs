// Tool definitions for the MCP Server
// This module defines custom tools that are exposed through the MCP protocol

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCP.Demo.Web.Tools
{
    /// <summary>
    /// MCP Server tool class that provides API-related operations
    /// This class is automatically registered as an MCP tool due to the McpServerToolType attribute
    /// </summary>
    [McpServerToolType]
    public sealed class MyApiTools
    {
        // HTTP client factory for making external API calls
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor that receives HTTP client factory via dependency injection
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating named HTTP clients</param>
        public MyApiTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Retrieves employee information based on the provided name
        /// This method is exposed as an MCP tool and can be called by MCP clients
        /// </summary>
        /// <param name="name">The name of the employee to look up</param>
        /// <returns>Employee information as a string</returns>
        [McpServerTool]
        [Description("Get the employee information from the employee API.")]
        [McpMeta("category", "employee")]
        public async Task<string> GetApiResponse(string name)
        {
            // Code below demonstrates the actual API call pattern
            // Currently using a mock response, but can be enabled to call real API
            //var client = _httpClientFactory.CreateClient("MyApi");
            //var responseString = await client.GetStringAsync($"/employees/{name}");

            // Mock response for demonstration purposes
            var responseString = $"My name is {name}";
            
            return responseString;
        }
    }
}
