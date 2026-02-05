using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCP.Demo.Web.Tools
{
    [McpServerToolType]
    public sealed class MyApiTools
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public MyApiTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool, Description("Get the employee information from the employee API.")]
        [McpMeta("category", "employee")]
        public async Task<string> GetApiResponse(string name)
        {
            //var client = _httpClientFactory.CreateClient("MyApi");
            //var responseString = await client.GetStringAsync($"/employees/{name}");

            var responseString = $"My name is {name}";
            
            return responseString;
        }

    }
}
