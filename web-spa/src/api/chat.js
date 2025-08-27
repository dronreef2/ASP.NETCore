// Exemplo de integração React/JS com o endpoint /api/chat
export async function sendChat(message, userId) {
  const response = await fetch('http://localhost:8080/api/chat', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ message, userId })
  });
  return response.json();
}
