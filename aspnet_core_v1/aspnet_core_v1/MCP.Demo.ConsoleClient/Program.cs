// See https://aka.ms/new-console-template for more information
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

Console.WriteLine("Waiting for MCP server to start. Press enter once the MCP server is up and running.");

Console.ReadLine();

var transportClient = new HttpClientTransport(new HttpClientTransportOptions()
{
    Endpoint = new Uri("http://localhost:5231/"),
    
});

Console.WriteLine("Connecting to the MCP server");
await using var mcpClient = await McpClient.CreateAsync(transportClient);

Console.WriteLine("Listing available tools from MCP server");
var mcpTools = await mcpClient.ListToolsAsync();
foreach(var m in mcpTools)
{
    Console.WriteLine(m.Name);
}

Dictionary<string, object> parameters = new();
parameters.Add("name", "Hardik");

var toolResult = await mcpTools[0].CallAsync(parameters);

Console.WriteLine("Printing the tool response");

foreach(var c in toolResult.Content)
{
    if(c.Type.Equals("text", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine((c as TextContentBlock).Text);
    }
}


Console.WriteLine();
Console.WriteLine();

Console.WriteLine("Processign done");

Console.ReadLine();