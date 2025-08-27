// Serviço Angular para integração com /api/chat
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ChatService {
  constructor(private http: HttpClient) {}

  sendChat(message: string, userId: string): Observable<any> {
    return this.http.post('http://localhost:8080/api/chat', { message, userId });
  }
}
