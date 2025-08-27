import { Component } from '@angular/core';
import { ChatService } from './chat.service';

@Component({
  selector: 'app-chat-form',
  template: `
    <div style="max-width:500px;margin:40px auto;padding:24px;border-radius:12px;box-shadow:0 2px 16px #0002;background:#fff;">
      <h2 style="text-align:center;margin-bottom:24px;">Chat Tutor Copiloto</h2>
      <form (ngSubmit)="sendMessage()" style="display:flex;gap:8px;">
        <input
          type="text"
          [(ngModel)]="message"
          name="message"
          placeholder="Digite sua mensagem..."
          style="flex:1;padding:12px;border-radius:8px;border:1px solid #ccc;font-size:16px;"
          required
        />
        <button type="submit" [disabled]="loading" style="padding:0 24px;border-radius:8px;background:#007bff;color:#fff;border:none;font-size:16px;">
          {{ loading ? 'Enviando...' : 'Enviar' }}
        </button>
      </form>
      <div *ngIf="response" style="margin-top:24px;background:#f6f8fa;padding:16px;border-radius:8px;min-height:60px;">
        <strong>Resposta:</strong>
        <div style="margin-top:8px;white-space:pre-line;">{{ response }}</div>
      </div>
    </div>
  `
})
export class ChatFormComponent {
  message = '';
  response = '';
  loading = false;

  constructor(private chatService: ChatService) {}

  sendMessage() {
    this.loading = true;
    this.response = '';
    this.chatService.sendChat(this.message, 'user1').subscribe({
      next: (result) => {
        this.response = result.response || JSON.stringify(result);
        this.loading = false;
      },
      error: () => {
        this.response = 'Erro ao enviar mensagem.';
        this.loading = false;
      }
    });
  }
}
