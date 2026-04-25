import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ProdutoService } from '../../core/services/produto.service';
import { MarcaService } from '../../core/services/marca.service';
import { Produto } from '../../core/models/produto.model';
import { Marca } from '../../core/models/marca.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './produtos.component.html',
  styleUrl: './produtos.component.scss'
})
export class ProdutosComponent implements OnInit {
  readonly lista               = signal<Produto[]>([]);
  readonly marcas              = signal<Marca[]>([]);
  readonly carregando          = signal(false);
  readonly modalAberto         = signal(false);
  readonly salvando            = signal(false);
  readonly erro                = signal('');
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
    private service: ProdutoService,
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
    this.carregarMarcas();
    this.modalAberto.set(true);
  }

  abrirEditar(item: Produto): void {
    this.editandoId = item.id;
    this.erro.set('');
    this.dropdownMarcaAberto.set(false);
    this.carregarMarcas(() => {
      this.form.setValue({ nome: item.nome, idMarca: item.idMarca });
      const marca = this.marcas().find(m => m.id === item.idMarca);
      this.buscaMarca.set(marca?.nome ?? '');
    });
    this.modalAberto.set(true);
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
    // restaura o nome da marca selecionada se o campo perder foco sem seleção
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

  private carregarMarcas(callback?: () => void): void {
    this.marcaService.listar().subscribe({
      next: marcas => { this.marcas.set(marcas); callback?.(); }
    });
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
