using YamlDotNet.Serialization;

public class InstructionLoader
{
    public InstructionData LoadInstruction(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var promptsPath = Path.Combine(basePath, "prompts", "prompt_template.md");
        var instructionPath = Path.Combine(basePath, "instructions", fileName);

        // Validate required files exist
        if (!File.Exists(promptsPath))
            throw new FileNotFoundException("Prompt template file not found. Ensure prompt_template.md exists in the prompts directory.");
        
        if (!File.Exists(instructionPath))
            throw new FileNotFoundException($"Instruction file '{fileName}' not found in the instructions directory.");

        // Read the instruction file
        var instructionContent = File.ReadAllText(instructionPath);
        
        // Parse front matter and content
        var (metadata, content) = ParseFrontMatter(instructionContent);
        
        // Load and process prompt template
        var promptTemplate = File.ReadAllText(promptsPath)
            .Replace("{{DOMAIN_NAME}}", metadata.Domain)
            .Replace("{{TONE_STYLE}}", metadata.Tone);

        // Combine prompt template with instruction content
        var combinedContent = $"{promptTemplate}\n\n# Knowledge Base\n{content}";
        
        return new InstructionData
        {
            Metadata = metadata,
            Content = combinedContent
        };
    }
    
    private (InstructionMetadata metadata, string content) ParseFrontMatter(string fileContent)
    {
        var metadata = new InstructionMetadata();
        var content = fileContent;
        
        if (fileContent.StartsWith("---"))
        {
            var endOfFrontMatter = fileContent.IndexOf("---", 3);
            if (endOfFrontMatter > 0)
            {
                var frontMatterYaml = fileContent.Substring(4, endOfFrontMatter - 4).Trim();
                content = fileContent.Substring(endOfFrontMatter + 3).Trim();
                
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                        .Build();
                    metadata = deserializer.Deserialize<InstructionMetadata>(frontMatterYaml) ?? new InstructionMetadata();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse front matter: {ex.Message}", ex);
                }
            }
        }
        
        return (metadata, content);
    }
}