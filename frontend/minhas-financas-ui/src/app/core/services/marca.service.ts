import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Marca, MarcaRequest } from '../models/marca.model';

@Injectable({ providedIn: 'root' })
export class MarcaService {
  private readonly url = `${environment.apiUrl}/marcas`;

  constructor(private http: HttpClient) {}

  listar(busca?: string): Observable<Marca[]> {
    const params = busca ? new HttpParams().set('busca', busca) : undefined;
    return this.http.get<Marca[]>(this.url, { params });
  }

  criar(req: MarcaRequest): Observable<Marca> {
    return this.http.post<Marca>(this.url, req);
  }

  atualizar(id: number, req: MarcaRequest): Observable<Marca> {
    return this.http.put<Marca>(`${this.url}/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
