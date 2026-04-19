import { Component } from '@angular/core';

@Component({
  selector: 'app-despesas',
  standalone: true,
  template: `
    <div class="pagina-placeholder">
      <h2>Despesas</h2>
      <p>Em construção.</p>
    </div>
  `,
  styles: [`
    .pagina-placeholder {
      background: #fff;
      border-radius: 12px;
      padding: 40px;
      h2 { font-size: 20px; margin-bottom: 8px; }
      p  { color: #888; }
    }
  `]
})
export class DespesasComponent {}
