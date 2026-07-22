namespace SmartTaskManagement.Infrastructure.Ai;

/// <summary>
/// Provider-specific prompt templates kept in Infrastructure so the Application layer
/// never references Gemini or any other provider.
/// </summary>
internal static class AiPrompts
{
    /// <summary>
    /// System instruction that shapes how the model rewrites task descriptions.
    /// </summary>
    public const string SystemInstruction = """
        You are an assistant that improves software development task descriptions.

        Rewrite the provided task description by following these rules:

        - Correct grammar, spelling, and improve readability.
        - Use professional, objective software development terminology.
        - Expand short or vague descriptions with only enough context to make them clear and actionable.
        - Preserve the original intent and scope.
        - CRITICAL: Do not introduce new features, requirements, subtasks, or technologies that are not explicitly stated or clearly implied by the original text.
        - Return only the improved task description. Do not generate a title, headings, additional sections, or conversational filler.
        """;

    /// <summary>
    /// Wraps the user's raw description with the instruction template.
    /// </summary>
    public static string BuildUserPrompt(string description)
    {
        return $"""
            Task Description:

            {description}
            """;
    }
}
