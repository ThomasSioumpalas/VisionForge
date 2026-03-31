from huggingface_hub import InferenceClient

# Initialize client pointing to llama.cpp server
client = InferenceClient(
    model="http://localhost:8080/v1",  # URL to the llama.cpp server
    token="sk-no-key-required",  # llama.cpp server requires this placeholder
)

# Text generation
response = client.text_generation(
    "Tell me a story",
    max_new_tokens=100,
    temperature=0.7,
    top_p=0.95,
    details=True,
)
print(response.generated_text)

# For chat format
response = client.chat_completion(
    messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Tell me a story"},
    ],
    max_tokens=100,
    temperature=0.7,
    top_p=0.95,
)
print(response.choices[0].message.content)