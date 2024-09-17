namespace AIDogConsole;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;

internal class AIDogPlugin
{
    private readonly AIDogClient _client;

    public AIDogPlugin(AIDogClient client)
    {
        this._client = client;
    }

    [KernelFunction, Description("Describes what the dog is able to see in front of him with details")]
    public async Task<string> SightAsync(Kernel kernel)
    {
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var imageBytes = await this._client.GetSightAsync();

        ChatHistory chatHistory = new ChatHistory(
            """
            You are a tiny walking dog with 10cm height and 10cm wide that can move around only in a perfectly flat environment, all information obtained from images should be only related to movement possibilities. 
            When providing the details, please also consider the amount of space in centimeters or degrees (for turning) that may be needed to perform the described movements and avoid as much as possible any blocking objects, like walls, obstacles, ramps, or stairs.
            If any blocking objects are present, or the image is blurred due to very close proximity in front of you, don't give forward as an option.
            """)
        {
            new ChatMessageContent 
            {
                Role = AuthorRole.User,
                Items = [
                new TextContent("Describe the moving options available in this image"),
                new ImageContent(imageBytes, "image/jpeg")]
            }
        };

        var response = await chatCompletion.GetChatMessageContentAsync(chatHistory);

        return response.ToString();
    }

    [KernelFunction, Description("Moves forward the amount of centimeters provided and describes what the dog is able to see in front of him after the movement was performed")]
    public async Task<string> MoveForwardAsync(
        Kernel kernel,
        [Description("Amount of centimeters to move forward")] int distanceInCentimeters)
    {
        await this._client.MoveForwardAsync(distanceInCentimeters);

        return await this.SightAsync(kernel);
    }

    [KernelFunction, Description("Moves backwards the amount of centimeters provided and describes what the dog is able to see in front of him after the movement was performed")]
    public async Task<string> MoveBackwardAsync(
        Kernel kernel,
        [Description("Amount of centimeters to move forward")] int distanceInCentimeters)
    {
        await this._client.MoveForwardAsync(-distanceInCentimeters);

        return await this.SightAsync(kernel);
    }

    [KernelFunction, Description("Turns left the amount of degrees provided and describes what the dog is able to see in front of him after the movement was performed")]
    public async Task<string> TurnLeftAsync(
        Kernel kernel,
        [Description("Amount of degrees to turn left")] int degrees)
    {
        await this._client.TurnLeftAsync(degrees);
        return await this.SightAsync(kernel);
    }

    [KernelFunction, Description("Turns right the amount of degrees provided and describes what the dog is able to see in front of him after the movement was performed")]
    public async Task<string> TurnRightAsync(
        Kernel kernel,
        [Description("Amount of degrees to turn right")] int degrees)
    {
        await this._client.TurnLeftAsync(-degrees);
        return await this.SightAsync(kernel);
    }
}
