// Script de teste automatizado para o endpoint /api/chat
import { sendChat } from './chat.js';

(async () => {
  const result = await sendChat('Olá, backend!', 'user1');
  console.log('Resposta do backend:', result);
})();
