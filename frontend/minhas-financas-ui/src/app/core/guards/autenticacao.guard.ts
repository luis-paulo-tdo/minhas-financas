import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AutenticacaoService } from '../services/autenticacao.service';

export const autenticacaoGuard: CanActivateFn = () => {
  const auth   = inject(AutenticacaoService);
  const router = inject(Router);

  if (auth.estaAutenticado()) return true;

  router.navigate(['/login']);
  return false;
};
