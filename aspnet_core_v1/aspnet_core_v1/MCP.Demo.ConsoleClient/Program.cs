// MCP (Model Context Protocol) Demo Console Client Application
// This application demonstrates how to connect to and interact with an MCP server
// It retrieves available tools from the server and executes one of them

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Prompt user to ensure MCP server is running before establishing connection
Console.WriteLine("Waiting for MCP server to start. Press enter once the MCP server is up and running.");
Console.ReadLine();

// Initialize HTTP transport for communicating with MCP server
// The server is expected to be running on localhost:5231
var transportClient = new HttpClientTransport(new HttpClientTransportOptions()
{
    Endpoint = new Uri("http://localhost:5231/"),
});

// Establish connection to MCP server
Console.WriteLine("Connecting to the MCP server");
await using var mcpClient = await McpClient.CreateAsync(transportClient);

// Retrieve and display all available tools from the MCP server
Console.WriteLine("Listing available tools from MCP server");
var mcpTools = await mcpClient.ListToolsAsync();
foreach(var m in mcpTools)
{
    Console.WriteLine(m.Name);
}

// Prepare input parameters for the first available tool
Dictionary<string, object> parameters = new();
parameters.Add("name", "Hardik");

// Execute the first tool with the provided parameters
var toolResult = await mcpTools[0].CallAsync(parameters);

// Display the tool response to console
Console.WriteLine("Printing the tool response");

// Iterate through response content blocks and print text content
foreach(var c in toolResult.Content)
{
    // Only process text content blocks
    if(c.Type.Equals("text", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine((c as TextContentBlock).Text);
    }
}

// Add spacing before completion message
Console.WriteLine();
Console.WriteLine();

// Indicate processing completion
Console.WriteLine("Processign done");

// Wait for user input before closing application
Console.ReadLine();