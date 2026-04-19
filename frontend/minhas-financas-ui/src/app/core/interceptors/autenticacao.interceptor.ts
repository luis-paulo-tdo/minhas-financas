import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AutenticacaoService } from '../services/autenticacao.service';

export const autenticacaoInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AutenticacaoService).obterToken();

  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
