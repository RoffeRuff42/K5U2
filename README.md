# AI Content Assistant 2.0

A microservice architecture with two APIs. Built for secure communication, robust error handling, and separation of concerns.

## Structure
* **Service A (Content API):** User-facing backend. Receives prompts, calls the proxy API, and saves the results in a database.
* **Service B (LLM Proxy API):** Proxy that handles external HTTP requests. Access is protected by a built-in X-API-KEY. 

*(Note: The application is temporarily connected to a public Joke API due to network blocks against AI services. However, the architecture works exactly the same regardless of the endpoint).*

## Error Handling (Graceful Degradation)
Service B uses a Custom Exception Middleware. If external APIs fail (e.g., Timeout, 429 Too Many Requests, 404), the application does not crash. The error is converted into a standardized ProblemDetails response (JSON) that Service A can handle gracefully.

## API Keys & Security
*Although the Joke API does not require keys, the system is set up to handle secrets securely:*

**Local (During Development)**
Secrets (like OpenAI keys) are never stored in appsettings.json. Instead, .NET User Secrets are used.
In the terminal (inside the Service B folder), run:
1. dotnet user-secrets init
2. dotnet user-secrets set "ApiKeys:ExternalLlm" "your-key-here"

**Production (Deployment)**
In a production environment (e.g., Azure/AWS), Environment Variables are used. The key is configured in the cloud service and loaded automatically, preventing secrets from ending up in version control (Git).
