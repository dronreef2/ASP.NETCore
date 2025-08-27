export async function explainWithLlama(query, indexName) {
  const response = await fetch('/api/llama/explain', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query, indexName })
  });
  const data = await response.json();
  return data.result;
}
