import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { Usuario } from '../models/usuario.model';

const TOKEN_KEY  = 'mf_token';
const USUARIO_KEY = 'mf_usuario';

@Injectable({ providedIn: 'root' })
export class AutenticacaoService {
  private readonly _usuario = signal<Usuario | null>(this.carregarUsuario());

  readonly usuario = this._usuario.asReadonly();

  constructor(private http: HttpClient, private router: Router) {}

  login(req: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/autenticacao/login`, req).pipe(
      tap(res => {
        localStorage.setItem(TOKEN_KEY, res.token);
        const usuario: Usuario = { nome: res.nome, email: res.email, dataExpiracao: res.dataExpiracao };
        localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario));
        this._usuario.set(usuario);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    this._usuario.set(null);
    this.router.navigate(['/login']);
  }

  obterToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  estaAutenticado(): boolean {
    const token = this.obterToken();
    if (!token) return false;
    const usuario = this._usuario();
    if (!usuario) return false;
    return new Date(usuario.dataExpiracao) > new Date();
  }

  private carregarUsuario(): Usuario | null {
    const json = localStorage.getItem(USUARIO_KEY);
    return json ? JSON.parse(json) : null;
  }
}
