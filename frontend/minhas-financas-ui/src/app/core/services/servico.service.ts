import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Servico, ServicoRequest } from '../models/servico.model';

@Injectable({ providedIn: 'root' })
export class ServicoService {
  private readonly url = `${environment.apiUrl}/servicos`;

  constructor(private http: HttpClient) {}

  listar(busca?: string): Observable<Servico[]> {
    const params = busca ? new HttpParams().set('busca', busca) : undefined;
    return this.http.get<Servico[]>(this.url, { params });
  }

  obter(id: number): Observable<Servico> {
    return this.http.get<Servico>(`${this.url}/${id}`);
  }

  criar(req: ServicoRequest): Observable<Servico> {
    return this.http.post<Servico>(this.url, req);
  }

  atualizar(id: number, req: ServicoRequest): Observable<Servico> {
    return this.http.put<Servico>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
