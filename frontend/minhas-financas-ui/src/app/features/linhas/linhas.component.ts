import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { LinhaProdutoService } from '../../core/services/linha-produto.service';
import { MarcaService } from '../../core/services/marca.service';
import { LinhaProduto } from '../../core/models/linha-produto.model';
import { Marca } from '../../core/models/marca.model';

@Component({
  selector: 'app-linhas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './linhas.component.html',
  styleUrl: './linhas.component.scss'
})
export class LinhasComponent implements OnInit {
  readonly lista               = signal<LinhaProduto[]>([]);
  readonly marcas              = signal<Marca[]>([]);
  readonly carregando          = signal(false);
  readonly modalAberto         = signal(false);
  readonly salvando            = signal(false);
  readonly erro                = signal('');
  readonly erroExclusao        = signal('');
  readonly confirmarExclusaoId = signal<number | null>(null);

  readonly buscaMarca          = signal('');
  readonly dropdownMarcaAberto = signal(false);
  readonly marcasFiltradas     = computed(() => {
    const termo = this.buscaMarca().toLowerCase().trim();
    if (!termo) return this.marcas();
    return this.marcas().filter(m => m.nome.toLowerCase().includes(termo));
  });

  editandoId: number | null = null;
  form: FormGroup;

  constructor(
    private service: LinhaProdutoService,
    private marcaService: MarcaService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      nome:    ['', [Validators.required, Validators.minLength(2)]],
      idMarca: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.carregar();
    this.marcaService.listar().subscribe(m => this.marcas.set(m));
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
    this.buscaMarca.set('');
    this.dropdownMarcaAberto.set(false);
    this.modalAberto.set(true);
  }

  abrirEditar(item: LinhaProduto): void {
    this.editandoId = item.id;
    this.erro.set('');
    this.dropdownMarcaAberto.set(false);
    this.form.setValue({ nome: item.nome, idMarca: item.idMarca });
    this.buscaMarca.set(item.nomeMarca);
    this.modalAberto.set(true);
  }

  fecharModal(): void {
    this.modalAberto.set(false);
    this.dropdownMarcaAberto.set(false);
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.salvando.set(true);
    this.erro.set('');

    const req = { nome: this.form.value.nome, idMarca: Number(this.form.value.idMarca) };

    const op$ = this.editandoId
      ? this.service.atualizar(this.editandoId, req)
      : this.service.criar(req);

    op$.subscribe({
      next: () => { this.fecharModal(); this.carregar(); this.salvando.set(false); },
      error: (err: HttpErrorResponse) => {
        this.erro.set(err.error?.mensagem ?? 'Erro ao salvar. Tente novamente.');
        this.salvando.set(false);
      }
    });
  }

  pedirConfirmacaoExclusao(id: number): void {
    this.erroExclusao.set('');
    this.confirmarExclusaoId.set(id);
  }

  cancelarExclusao(): void {
    this.confirmarExclusaoId.set(null);
    this.erroExclusao.set('');
  }

  confirmarExclusao(): void {
    const id = this.confirmarExclusaoId();
    if (id === null) return;

    this.service.excluir(id).subscribe({
      next: () => { this.confirmarExclusaoId.set(null); this.carregar(); },
      error: (err: HttpErrorResponse) => {
        const msg = err.status === 500 || err.status === 409
          ? 'Não é possível excluir esta linha pois ela está vinculada a um ou mais produtos.'
          : (err.error?.mensagem ?? 'Erro ao excluir. Tente novamente.');
        this.erroExclusao.set(msg);
      }
    });
  }

  onBusca(event: Event): void {
    const valor = (event.target as HTMLInputElement).value;
    this.carregar(valor || undefined);
  }

  selecionarMarca(marca: Marca): void {
    this.form.get('idMarca')!.setValue(marca.id);
    this.buscaMarca.set(marca.nome);
    this.dropdownMarcaAberto.set(false);
  }

  abrirDropdownMarca(): void {
    this.buscaMarca.set('');
    this.form.get('idMarca')!.setValue(null);
    this.dropdownMarcaAberto.set(true);
  }

  fecharDropdownMarca(): void {
    setTimeout(() => {
      const idMarca = this.form.get('idMarca')?.value;
      if (!idMarca) {
        this.buscaMarca.set('');
      } else {
        const marca = this.marcas().find(m => m.id === idMarca);
        this.buscaMarca.set(marca?.nome ?? '');
      }
      this.dropdownMarcaAberto.set(false);
    }, 150);
  }
}
