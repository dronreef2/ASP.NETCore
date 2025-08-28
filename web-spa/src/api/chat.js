// API client para chat com o backend
export async function sendChat(message, userId) {
  try {
    const response = await fetch('/api/chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        message: message,
        userId: userId || 'anonymous'
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Erro na API de chat:', error);
    throw error;
  }
}

// Função para obter modelos disponíveis
export async function getAvailableModels() {
  try {
    const response = await fetch('/api/chat/models');

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Erro ao obter modelos:', error);
    throw error;
  }
}
