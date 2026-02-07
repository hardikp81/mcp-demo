using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using MCP.Demo.ConsoleClient2;
using Microsoft.AI.Foundry.Local;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

CancellationToken ct = new CancellationToken();

#region SETTING UP LOCAL LLM SERVER USING FOUNDRY LOCAL

Console.WriteLine("START Setting up local LLM server using Foundry Local");

var config = new Configuration { 
    AppName = "foundry_local_samples",
    LogLevel = Microsoft.AI.Foundry.Local.LogLevel.Information
};


await FoundryLocalManager.CreateAsync(config, Utils.GetAppLogger());


var mgr = FoundryLocalManager.Instance;


Console.WriteLine("Start registering execution providers");

// Ensure that any Execution Provider (EP) downloads run and are completed.
// EP packages include dependencies and may be large.
// Download is only required again if a new version of the EP is released.
// For cross platform builds there is no dynamic EP download and this will return immediately.
await Utils.RunWithSpinner("Registering execution providers", mgr.EnsureEpsDownloadedAsync());


Console.WriteLine("Getting model catalog");
// Get the model catalog
var catalog = await mgr.GetCatalogAsync();


// Get a model using an alias.
var model = await catalog.GetModelAsync("qwen2.5-14b") ?? throw new Exception("Model not found");

var isModelCached = await model.IsCachedAsync();

Console.WriteLine($"Model {model.Id} is {(isModelCached ? "already cached" : "not cached")} locally.");

if(isModelCached == false)
{
    Console.WriteLine($"Start downloading model: {model.Id}");
    // Download the model (the method skips download if already cached)
    await model.DownloadAsync(progress =>
    {
        Console.Write($"\rDownloading model: {progress:F2}%");
        if (progress >= 100f)
        {
            Console.WriteLine();
        }
    });
}

// Load the model
Console.Write($"Loading model {model.Id}...");
await model.LoadAsync();
Console.WriteLine("Model loading completed.");

// Get a chat client
var chatClient = await model.GetChatClientAsync();

// Create a chat message
List<ChatMessage> messages = new()
{
    new ChatMessage { Role = "user", Content = "Why is the sky blue?" }
};

// Get a streaming chat completion response
Console.WriteLine("Chat completion response:");
var streamingResponse = chatClient.CompleteChatStreamingAsync(messages, ct);
await foreach (var chunk in streamingResponse)
{
    Console.Write(chunk.Choices[0].Message.Content);
    Console.Out.Flush();
}
Console.WriteLine();

Console.WriteLine("Unloading Model");
// Tidy up - unload the model
await model.UnloadAsync();

#endregion

#region CALLING MCP SERVER TOOLS


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
foreach (var m in mcpTools)
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
foreach (var c in toolResult.Content)
{
    // Only process text content blocks
    if (c.Type.Equals("text", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine((c as TextContentBlock).Text);
    }
}

#endregion

Console.WriteLine("Work is complete");

Console.ReadLine();
