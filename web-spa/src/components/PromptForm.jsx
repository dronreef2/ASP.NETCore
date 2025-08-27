import { useState } from 'react';
import { runPrompt } from '../api/ia';

export default function PromptForm() {
  const [prompt, setPrompt] = useState('');
  const [result, setResult] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const res = await runPrompt(prompt);
    setResult(res);
  };

  return (
    <form onSubmit={handleSubmit}>
      <textarea value={prompt} onChange={e => setPrompt(e.target.value)} placeholder="Digite seu prompt de IA..." />
      <button type="submit">Enviar</button>
      <div>Resposta: {result}</div>
    </form>
  );
}
