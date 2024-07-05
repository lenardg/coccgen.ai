using OpenAI;
using OpenAI.Chat;

namespace CoCCGen.Core;

public class OpenAIHelper(string apiKey) {

    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public int TotalTokens { get; private set; }

    public async Task<string> CreateCompletionAsync(string system, string prompt) {
        var client = GetChatClient();

        var messages = new ChatMessage[] {
            new SystemChatMessage(system),
            new UserChatMessage(prompt),
        };

        var response = await client.CompleteChatAsync(messages);

        InputTokens += response.Value.Usage.InputTokens;
        OutputTokens += response.Value.Usage.OutputTokens;
        TotalTokens += response.Value.Usage.TotalTokens;

        return response.Value.Content[0].Text;
    }

    private ChatClient GetChatClient() {
        OpenAIClient client = GetOpenAIClient(apiKey);
        return client.GetChatClient("gpt-4o");
    }

    private static OpenAIClient GetOpenAIClient(string apiKey) {
        return new OpenAIClient(apiKey);
    }
}
