import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Produto, ProdutoRequest } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly url = `${environment.apiUrl}/produtos`;

  constructor(private http: HttpClient) {}

  listar(busca?: string, idMarca?: number, idLinhaProduto?: number): Observable<Produto[]> {
    let params = new HttpParams();
    if (busca)          params = params.set('busca', busca);
    if (idMarca)        params = params.set('idMarca', idMarca);
    if (idLinhaProduto) params = params.set('idLinhaProduto', idLinhaProduto);
    return this.http.get<Produto[]>(this.url, { params });
  }

  criar(req: ProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.url, req);
  }

  atualizar(id: number, req: ProdutoRequest): Observable<Produto> {
    return this.http.put<Produto>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
