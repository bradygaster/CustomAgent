public class InstructionLoader
{
    public string Load(string domainName, string toneStyle)
    {
        var basePath = AppContext.BaseDirectory;
        var promptsPath = Path.Combine(basePath, "prompts", "prompt_template.md");
        var instructionsPath = Path.Combine(basePath, "instructions");

        // Validate required files exist
        if (!File.Exists(promptsPath))
            throw new FileNotFoundException("Prompt template file not found. Ensure prompt_template.md exists in the prompts directory.");

        var instructionFiles = Directory.GetFiles(instructionsPath, "*.md");
        if (instructionFiles.Length == 0)
            throw new FileNotFoundException("No instruction files found. Ensure at least one .md file exists in the instructions directory.");

        // Load and process instructions
        var promptTemplate = File.ReadAllText(promptsPath)
            .Replace("{{DOMAIN_NAME}}", domainName)
            .Replace("{{TONE_STYLE}}", toneStyle);

        var instructionsContent = string.Join("\n\n",
            instructionFiles.Select(file =>
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                return $"## {fileName}\n{content}";
            }));

        return $"{promptTemplate}\n\n# Knowledge Base\n{instructionsContent}";
    }
}