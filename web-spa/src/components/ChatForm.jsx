import React, { useState } from 'react';
import { sendChat } from '../api/chat';

export default function ChatForm() {
  const [message, setMessage] = useState('');
  const [response, setResponse] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setResponse('');
    try {
      const result = await sendChat(message, 'user1');
      setResponse(result.response || JSON.stringify(result));
    } catch (err) {
      setResponse('Erro ao enviar mensagem.');
    }
    setLoading(false);
  };

  return (
    <div style={{ maxWidth: 500, margin: '40px auto', padding: 24, borderRadius: 12, boxShadow: '0 2px 16px #0002', background: '#fff' }}>
      <h2 style={{ textAlign: 'center', marginBottom: 24 }}>Chat Tutor Copiloto</h2>
      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 8 }}>
        <input
          type="text"
          value={message}
          onChange={e => setMessage(e.target.value)}
          placeholder="Digite sua mensagem..."
          style={{ flex: 1, padding: 12, borderRadius: 8, border: '1px solid #ccc', fontSize: 16 }}
          required
        />
        <button type="submit" disabled={loading} style={{ padding: '0 24px', borderRadius: 8, background: '#007bff', color: '#fff', border: 'none', fontSize: 16 }}>
          {loading ? 'Enviando...' : 'Enviar'}
        </button>
      </form>
      {response && (
        <div style={{ marginTop: 24, background: '#f6f8fa', padding: 16, borderRadius: 8, minHeight: 60 }}>
          <strong>Resposta:</strong>
          <div style={{ marginTop: 8, whiteSpace: 'pre-line' }}>{response}</div>
        </div>
      )}
    </div>
  );
}
