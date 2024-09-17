using AIDogConsole;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKey = configuration["ApiKey"]!;
var robotEndpoint = new Uri(configuration["Endpoint"]!);

var services = new ServiceCollection();
services.AddSingleton(new AIDogClient(httpClient: HttpClientProvider.GetHttpClient(), endpoint: robotEndpoint))
    .AddKernel()
        .AddOpenAIChatCompletion("gpt-4o-mini", apiKey: apiKey)
        .Plugins
            .AddFromType<AIDogPlugin>();

var serviceProvider = services.BuildServiceProvider();
var kernel = serviceProvider.GetRequiredService<Kernel>();

var chatCompletion = serviceProvider.GetRequiredService<IChatCompletionService>();

var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

var aiSightDetail = (await kernel.InvokePromptAsync("What do you see?", new(settings))).ToString();

// var dogClient = serviceProvider.GetRequiredService<AIDogClient>();

Console.WriteLine(aiSightDetail);

int maxIterations = 5;
int currentIteration = 0;
while (currentIteration < maxIterations)
{
    ChatHistory commandChat = new ChatHistory(
    """
    You should select one of the options available by the assistant limited to 20cm for forward/backward movement and 20 degrees for turning movements.
    """);
    commandChat.AddUserMessage("Move away from any blocking objects, try to be in a clear space while moving");
    commandChat.AddAssistantMessage(aiSightDetail);
    commandChat.AddUserMessage("Select one of the available options");

    var aiCommand = await chatCompletion.GetChatMessageContentAsync(commandChat, settings);
    Console.WriteLine($"AI Command: {aiCommand}");
    aiSightDetail = (await kernel.InvokePromptAsync(aiCommand.Content!, new(settings))).ToString();
    
    Console.WriteLine($"Sigh Observation{aiSightDetail}");
    currentIteration++;
}
