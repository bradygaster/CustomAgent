public class AgentSettings
{
    public string Name { get; set; } = "Custom AI Agent";
    public string Domain { get; set; } = "the specified domain";
    public string ToneStyle { get; set; } = "scholarly but approachable";
}

public class UISettings
{
    public string WelcomeMessage { get; set; } = "AI Agent Console";
    public string PromptMessage { get; set; } = "Ask a question about the subject (type 'exit' to quit):";
}

public class AzureSettings
{
    public string ModelName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}
