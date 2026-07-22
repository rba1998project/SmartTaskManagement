# AI Prompt Documentation

## Purpose

The Smart Task Management System includes an AI-powered task description improver.

Its goal is to transform a short or poorly written task description into a clearer, more professional, and more actionable version while preserving the original intent.

The AI is an assistant only. Users review the generated text before deciding whether to save it.

---

## Prompt Design

The prompt follows a simple two-part structure:

1. **System instruction** — constant rules that shape every response.
2. **User prompt** — the raw user description wrapped in a small template.

The system instruction is:

```
You are an assistant that improves software development task descriptions.

Rewrite the provided task description by following these rules:

- Correct grammar, spelling, and improve readability.
- Use professional, objective software development terminology.
- Expand short or vague descriptions with only enough context to make them clear and actionable.
- Preserve the original intent and scope.
- CRITICAL: Do not introduce new features, requirements, subtasks, or technologies that are not explicitly stated or clearly implied by the original text.
- Return only the improved task description. Do not generate a title, headings, additional sections, or conversational filler.
```

The user prompt template is:

```
Task Description:

{userDescription}
```

The final payload concatenates the system instruction and the user prompt with a blank line between them.

---

## Design Rationale

The prompt intentionally favors improving the existing description rather than generating new requirements. This helps preserve the user's original intent while producing clearer and more actionable task descriptions.

## Example Inputs and Outputs

| Input | Output |
|-------|--------|
| `implement user auth` | `Implement user authentication functionality to allow users to securely log in and verify their identity, enabling access control to protected application resources.` |
| `fix bug when user cant update there profile after changing email.` | `Investigate and resolve an issue that prevents users from updating their profile information after changing their email address.` |
| `add serch in task page so user can find task fast.` | `Implement search functionality on the task page to allow users to quickly search for and locate specific tasks.` |

---

## Validation Approach

- **Input validation:** The request requires a non-empty `Description` with a maximum length of 2000 characters. Validation is enforced using FluentValidation before the AI service is called.
- **Output validation:** The improved description is treated as untrusted plain text. The backend does not render HTML, return Markdown, or expose provider-specific payloads.
- **Failure handling:** Provider errors, timeouts, missing API key, and invalid responses are all converted into the existing `Result` failure pattern with generic user-facing messages.

---

## Safety Considerations

- **Untrusted output:** AI-generated text is treated as untrusted. The frontend should render it as plain text, not as HTML.
- **No secrets:** The API key is stored in User Secrets / environment variables and is never exposed to the client.
- **No provider leakage:** Error messages returned to the client do not contain provider-specific details, stack traces, or raw API responses.
- **Optional feature:** The AI feature is disabled when the API key is missing. The application starts successfully and the endpoint returns a clear failure message.
- **Rate limits:** The existing global rate limiter protects the endpoint. There is no AI-specific rate limiting in this phase.

---

## Provider Independence

The application depends only on the `ITaskAiService` abstraction in the Application layer.

The current implementation uses Google Gemini via REST, but the architecture allows replacing it with another provider (for example OpenAI or Azure OpenAI) without changing controllers or application services.

---

## Assumptions and Limitations

- **Provider:** The implementation targets the Google Gemini REST API. The configured model can be changed through application configuration, while replacing the provider requires a new `ITaskAiService` implementation and a DI registration change.
- **Network dependency:** Description improvement requires outbound HTTPS access to the AI provider. Network failures or provider unavailability will prevent description improvement.
- **Plain-text only:** Although the AI provider may return additional metadata, the application returns only the improved description string to the client.
- **No persistence:** Improved descriptions are returned to the caller but are not automatically saved to any task record.
- **Provider variability:** AI-generated output is non-deterministic and may vary between requests or providers while still following the prompt instructions.
