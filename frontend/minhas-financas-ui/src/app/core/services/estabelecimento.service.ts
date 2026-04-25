import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Estabelecimento, EstabelecimentoRequest } from '../models/estabelecimento.model';

@Injectable({ providedIn: 'root' })
export class EstabelecimentoService {
  private readonly url = `${environment.apiUrl}/estabelecimentos`;

  constructor(private http: HttpClient) {}

  listar(busca?: string): Observable<Estabelecimento[]> {
    const params = busca ? new HttpParams().set('busca', busca) : undefined;
    return this.http.get<Estabelecimento[]>(this.url, { params });
  }

  criar(req: EstabelecimentoRequest): Observable<Estabelecimento> {
    return this.http.post<Estabelecimento>(this.url, req);
  }

  atualizar(id: number, req: EstabelecimentoRequest): Observable<Estabelecimento> {
    return this.http.put<Estabelecimento>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
