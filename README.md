# AI Content Assistant 2.0

A microservice architecture with two APIs. Built for secure communication, robust error handling, and separation of concerns.

## Structure
* **Service A (Content API):** User-facing backend. Receives prompts, calls the proxy API, and saves the results in a database.
* **Service B (LLM Proxy API):** Proxy that handles external HTTP requests. Access is protected by a built-in X-API-KEY. Forwards prompts to a locally hosted LLM via [Ollama](https://ollama.com/).

## Error Handling (Graceful Degradation)
Service B uses a Custom Exception Middleware. If the external LLM call fails (e.g., Timeout, 429 Too Many Requests, 404), the application does not crash. The error is converted into a standardized ProblemDetails response (JSON), logged internally, and Service A falls back to a safe, neutral message instead of exposing internal error details.

## API Keys & Security
Service B only accepts calls carrying the correct internal API key, compared using a constant-time check to prevent timing attacks. Secrets are never stored in appsettings.json:

**Local (During Development)**
.NET User Secrets are used. In the terminal, run:
1. `dotnet user-secrets init`
2. `dotnet user-secrets set "InternalApiKey" "your-key-here"` (inside the Service B folder)
3. `dotnet user-secrets set "ApiSettings:ApiKey" "your-key-here"` (inside the Service A folder, must match Service B's key)

**Production (Deployment)**
In a production environment (e.g., Azure/AWS), Environment Variables are used. The key is configured in the cloud service and loaded automatically, preventing secrets from ending up in version control (Git).
