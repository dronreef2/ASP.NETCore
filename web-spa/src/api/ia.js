export async function runPrompt(prompt) {
  const response = await fetch('/api/ia/prompt', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(prompt)
  });
  const data = await response.json();
  return data.result;
}
