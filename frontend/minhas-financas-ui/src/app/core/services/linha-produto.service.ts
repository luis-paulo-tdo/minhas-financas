import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LinhaProduto, LinhaProdutoRequest } from '../models/linha-produto.model';

@Injectable({ providedIn: 'root' })
export class LinhaProdutoService {
  private readonly url = `${environment.apiUrl}/linhas-produto`;

  constructor(private http: HttpClient) {}

  listar(busca?: string, idMarca?: number): Observable<LinhaProduto[]> {
    let params = new HttpParams();
    if (busca)   params = params.set('busca', busca);
    if (idMarca) params = params.set('idMarca', idMarca);
    return this.http.get<LinhaProduto[]>(this.url, { params });
  }

  criar(req: LinhaProdutoRequest): Observable<LinhaProduto> {
    return this.http.post<LinhaProduto>(this.url, req);
  }

  atualizar(id: number, req: LinhaProdutoRequest): Observable<LinhaProduto> {
    return this.http.put<LinhaProduto>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
