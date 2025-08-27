import { useState } from 'react';
import { explainWithLlama } from '../api/llama';

export default function LlamaExplainForm() {
  const [query, setQuery] = useState('');
  const [indexName, setIndexName] = useState('default-index');
  const [result, setResult] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setResult('');
    try {
      const res = await explainWithLlama(query, indexName);
      setResult(res);
    } catch (err) {
      setResult('Erro ao obter explicação.');
    }
    setLoading(false);
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        value={indexName}
        onChange={e => setIndexName(e.target.value)}
        placeholder="Nome do índice"
        style={{ marginBottom: 8 }}
      />
      <textarea
        value={query}
        onChange={e => setQuery(e.target.value)}
        placeholder="Digite sua dúvida ou código para explicação/refatoração..."
        rows={4}
        style={{ width: '100%', marginBottom: 8 }}
      />
      <button type="submit" disabled={loading}>
        {loading ? 'Processando...' : 'Enviar'}
      </button>
      <div style={{ marginTop: 16 }}>
        <strong>Resposta:</strong>
        <div>{result}</div>
      </div>
    </form>
  );
}
