import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ServicoService } from '../../core/services/servico.service';
import { Servico } from '../../core/models/servico.model';

@Component({
  selector: 'app-servicos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './servicos.component.html',
  styleUrl: './servicos.component.scss'
})
export class ServicosComponent implements OnInit {
  readonly lista               = signal<Servico[]>([]);
  readonly carregando          = signal(false);
  readonly modalAberto         = signal(false);
  readonly salvando            = signal(false);
  readonly erro                = signal('');
  readonly confirmarExclusaoId = signal<number | null>(null);

  editandoId: number | null = null;
  form: FormGroup;

  constructor(private service: ServicoService, private fb: FormBuilder) {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(busca?: string): void {
    this.carregando.set(true);
    this.service.listar(busca).subscribe({
      next: lista => { this.lista.set(lista); this.carregando.set(false); },
      error: ()   => this.carregando.set(false)
    });
  }

  abrirCriar(): void {
    this.editandoId = null;
    this.form.reset();
    this.erro.set('');
    this.modalAberto.set(true);
  }

  abrirEditar(item: Servico): void {
    this.editandoId = item.id;
    this.form.setValue({ nome: item.nome });
    this.erro.set('');
    this.modalAberto.set(true);
  }

  fecharModal(): void {
    this.modalAberto.set(false);
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.salvando.set(true);
    this.erro.set('');

    const op$ = this.editandoId
      ? this.service.atualizar(this.editandoId, this.form.value)
      : this.service.criar(this.form.value);

    op$.subscribe({
      next: () => { this.fecharModal(); this.carregar(); this.salvando.set(false); },
      error: (err: HttpErrorResponse) => {
        this.erro.set(err.error?.mensagem ?? 'Erro ao salvar. Tente novamente.');
        this.salvando.set(false);
      }
    });
  }

  pedirConfirmacaoExclusao(id: number): void {
    this.confirmarExclusaoId.set(id);
  }

  cancelarExclusao(): void {
    this.confirmarExclusaoId.set(null);
  }

  confirmarExclusao(): void {
    const id = this.confirmarExclusaoId();
    if (id === null) return;

    this.service.excluir(id).subscribe({
      next: () => { this.confirmarExclusaoId.set(null); this.carregar(); },
      error: () => this.confirmarExclusaoId.set(null)
    });
  }

  onBusca(event: Event): void {
    const valor = (event.target as HTMLInputElement).value;
    this.carregar(valor || undefined);
  }
}
