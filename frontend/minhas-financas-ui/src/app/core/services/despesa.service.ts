import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Dashboard, Despesa, DespesaRequest, FiltrosDespesa, ResultadoPaginado } from '../models/despesa.model';

@Injectable({ providedIn: 'root' })
export class DespesaService {
  private readonly url = `${environment.apiUrl}/despesas`;

  constructor(private http: HttpClient) {}

  listar(filtros: FiltrosDespesa = {}): Observable<ResultadoPaginado<Despesa>> {
    let params = new HttpParams();
    if (filtros.idCategoria)       params = params.set('idCategoria',       filtros.idCategoria);
    if (filtros.idEstabelecimento) params = params.set('idEstabelecimento', filtros.idEstabelecimento);
    if (filtros.idProduto)         params = params.set('idProduto',         filtros.idProduto);
    if (filtros.de)                params = params.set('de',                filtros.de);
    if (filtros.ate)               params = params.set('ate',               filtros.ate);
    if (filtros.pagina)            params = params.set('pagina',            filtros.pagina);
    if (filtros.tamanhoPagina)     params = params.set('tamanhoPagina',     filtros.tamanhoPagina);
    return this.http.get<ResultadoPaginado<Despesa>>(this.url, { params });
  }

  obter(id: number): Observable<Despesa> {
    return this.http.get<Despesa>(`${this.url}/${id}`);
  }

  criar(req: DespesaRequest): Observable<Despesa> {
    return this.http.post<Despesa>(this.url, req);
  }

  atualizar(id: number, req: DespesaRequest): Observable<Despesa> {
    return this.http.put<Despesa>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  obterDashboard(de: string, ate: string): Observable<Dashboard> {
    const params = new HttpParams().set('de', de).set('ate', ate);
    return this.http.get<Dashboard>(`${this.url}/dashboard`, { params });
  }
}
