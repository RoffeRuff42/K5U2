# Evaluation of AI Integration

## 1. Quality and Limitations
The system now generates content using a self-hosted, local LLM served through [Ollama](https://ollama.com/) (model: `llama3.2`, 3.2B parameters, Q4_K_M quantization). Service B (LLM Proxy API) calls Ollama's `api/generate` endpoint over HTTP on `localhost:11434`, so the entire pipeline — Service A, Service B, and the model itself — runs without any external network dependency or third-party API key. This replaces an earlier prototype that used a public Joke API as a stand-in while Hugging Face (blocked by a local DNS/network issue) and OpenAI (out of paid credits) were unreachable.

Running locally removes the cost and rate-limit constraints of cloud providers, and keeps all prompts and generated data on-device. The trade-off is model capability: a quantized 3B-parameter model is noticeably weaker than large frontier models (GPT-4-class or similar), and generation is CPU/GPU-bound by the local machine rather than elastically scaled cloud hardware. The general risks of LLM-generated content still apply regardless of where the model runs:

- **Relevance and correctness:** the model predicts plausible text based on probability, not verified truth. It can "hallucinate" — produce confident-sounding statements that are factually wrong.
- **Bias:** the model was trained on large internet-sourced datasets and can reproduce stereotypes or skewed perspectives present in that data.
- **Prompt sensitivity:** output quality depends heavily on how the prompt is phrased. A vague prompt tends to produce a vague or generic answer.
- **Latency:** local inference on consumer hardware is slow compared to cloud APIs. The measured generation time in Test 1 below (~6.1 seconds for one sentence) illustrates this; longer prompts or weaker hardware would take longer still, which is why Service B's `LlmClient` is configured with a 2-minute timeout instead of the short timeouts typical for cloud APIs.

## 2. Test Prompts and Results

All tests below were executed against the running services on 2026-06-19, with both APIs started via their HTTPS launch profiles and a real local Ollama instance serving `llama3.2:latest`.

### Test 1: Successful Request (Service A → Service B → Ollama)
**Input:** `POST /api/AiContent` on Service A with `{"title":"Keyboard blurb","originalPrompt":"Write a short, one-sentence product description for a wireless mechanical keyboard.","category":"Marketing"}`

**System handling:** Service A validated the DTO, called Service B's typed `LlmClient` with the prompt in the request body (`POST api/Llm/generate`), which authenticated with Service A's internal API key, called Ollama, and returned the generated text. Service A saved the result and returned it to the caller.

**Output:** `201 Created`, in ~6.1 seconds (`6142ms` per the `LogExecutionTimeAttribute` filter, confirmed in Service A's own log):
> "Introducing the WireFree Pro, a cutting-edge wireless mechanical keyboard that combines precision-tuned switches with advanced Bluetooth technology and long-lasting battery life, perfect for gamers, writers, and productivity enthusiasts alike."

**Conclusion:** The full chain — DTO validation, the typed-client call to Service B, the internal API key handshake, the call to Ollama, and persistence — works end to end with a real model response.

### Test 2: Security — Rejected Request (missing/invalid internal API key)
**Input:** `POST /api/Llm/generate` sent directly to Service B, first with no `X-API-KEY` header, then with an incorrect one (`WrongKey999`).

**System handling:** The `ApiKeyAttribute` action filter on `LlmController` compared the header against the configured `InternalApiKey` using a constant-time comparison (`CryptographicOperations.FixedTimeEquals`) before the action could run.

**Output:** Both requests were rejected with `401 Unauthorized` — `"Invalid internal API key."` — without ever reaching Ollama.

**Conclusion:** Service B only accepts calls that present the correct internal key, confirming Service B is not callable as an open public endpoint.

### Test 3: Graceful Degradation (upstream LLM failure)
**Input:** Service B was temporarily restarted with an invalid model name (`Llm:Model = "model-that-does-not-exist"`) to force a real failure from Ollama, then the same request from Test 1 was repeated through Service A.

**System handling:** Ollama returned `404 Not Found` for the unknown model. Service B's `ExceptionHandlingMiddleware` caught the resulting `HttpRequestException`, logged the full exception internally, and returned a generic `ProblemDetails` response instead of forwarding Ollama's raw error:
> `{"title":"External AI Service Error","status":404,"detail":"A network error occurred while communicating with the AI service."}`

Service A's `AiContentService` received that non-success response, logged the status code and body internally via `ILogger`, and — instead of writing any of that raw text into the saved record — stored the neutral fallback message. The request still completed successfully:
> `201 Created` — `"generatedText": "Content could not be generated at this time."`

**Conclusion:** When the external model is unavailable, neither service crashes and neither leaks internal error details (stack traces, upstream response bodies) to the end user — only a safe, neutral message is ever persisted or returned, while the real diagnostic detail goes to the server-side logs for debugging. Service B was restarted with the correct configuration immediately afterward and Test 1 was re-confirmed to still succeed.

## 3. Conclusion
Running the model locally through Ollama removes the cost, rate-limiting, and external-availability issues that blocked earlier testing against Hugging Face and OpenAI, at the cost of using a smaller, less capable model. The same architectural lessons still apply: AI-generated content should not be trusted for facts or sensitive decisions without verification, since even a well-integrated pipeline cannot fix hallucination or bias at the model level.

What the pipeline *can* guarantee — and what Tests 2 and 3 demonstrate — is operational safety: unauthorized callers are rejected, and failures in the external AI dependency degrade gracefully into a safe, generic message rather than crashing the service or exposing internal details to the end user.
