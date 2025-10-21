public class InstructionMetadata
{
    public string Name { get; set; } = "Custom AI Agent";
    public string Domain { get; set; } = "the specified domain";
    public string Tone { get; set; } = "scholarly but approachable";
    public string Welcome { get; set; } = "AI Agent Console";
    public string Prompt { get; set; } = "Ask a question (type 'exit' to quit):";
}

public class InstructionData
{
    public InstructionMetadata Metadata { get; set; } = new();
    public string Content { get; set; } = string.Empty;
}

public class AppSettings
{
    public string InstructionFile { get; set; } = "quantum.md";
}

public class AzureSettings
{
    public string ModelName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}
