# Evaluation of AI Integration

## 1. Quality and Limitations
Since my local network environment blocked requests to Hugging Face (DNS error) and OpenAI required paid credits (429 Too Many Requests), the system is currently connected to a public Joke API to prove that the underlying microservice architecture works. The evaluation of AI quality is therefore based on industry standards and theoretical knowledge of LLMs.

Relevance and Correctness: An AI model generates answers based on probability, not absolute truth. There is always a risk of "hallucinations", meaning the AI invents facts that sound convincing but are completely wrong.
Bias: AI models are trained on massive amounts of data from the internet. This means they can reflect human biases, stereotypes, or skews present in the training data.
Prompt Sensitivity: The quality of the AI's response is extremely dependent on the quality of the user's prompt. A vague and unclear question will result in an unspecific or poor answer. 

## 2. Test Prompts and Results

Below are tests conducted during development, proving that the system's integration and error handling (Graceful Degradation) work.

### Test 1: Successful Request (Joke API)
Input (Prompt): "Write a one sentence joke about a programmer."
System Handling: Service A received the request, verified the X-API-KEY, and forwarded it to Service B. Service B successfully called the external API.
Output: "How do you make the number one disappear? Add the letter G and it’s 'gone'!"
Conclusion: The API integration works. The response was formatted correctly and saved in the database.

### Test 2: Error Handling (OpenAI - Out of Credits)
Input (Prompt): "Write a one sentence joke about a programmer." (Sent to OpenAI's servers).
System Handling:OpenAI denied the request because the account lacked sufficient balance and returned status code 429 (Too Many Requests). Instead of the application crashing, my ExceptionHandlingMiddleware in Service B caught the error.
Output: A ProblemDetails response: `{"title":"External AI Service Error","status":429,"detail":"The external AI service is currently overloaded. Please wait a moment and try again."}`
Conclusion: This is a perfect example of Graceful Degradation. The system is robust enough to handle external network and API errors without crashing the end-user application (Service A).

## 3. Conclusion
Using external AI services is a great way to quickly generate text, summarize information, or brainstorm ideas. However, it shouldn't be used for anything that requires 100% guaranteed facts or sensitive decisions, because the AI can easily hallucinate or show bias. 
Also, relying on an external API means you absolutely need solid error handling. You are entirely dependent on someone else's servers being up and running, which my second test clearly proved.