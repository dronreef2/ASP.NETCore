// API client for chatting with the backend

export interface ChatMessage {
  message: string;
  userId: string;
}

export interface ChatResponse {
  response: string;
  // Add other properties from the API response as needed
}

export async function sendChat(message: string, userId: string = 'anonymous'): Promise<ChatResponse> {
  try {
    const response = await fetch('/api/chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        message: message,
        userId: userId,
      }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP error! status: ${response.status}, body: ${errorText}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error in chat API:', error);
    throw error;
  }
}

export async function getAvailableModels(): Promise<any> {
  try {
    const response = await fetch('/api/chat/models');

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching models:', error);
    throw error;
  }
}
