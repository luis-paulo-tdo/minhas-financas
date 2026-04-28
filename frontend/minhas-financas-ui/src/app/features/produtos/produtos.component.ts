import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ProdutoService } from '../../core/services/produto.service';
import { MarcaService } from '../../core/services/marca.service';
import { LinhaProdutoService } from '../../core/services/linha-produto.service';
import { Produto } from '../../core/models/produto.model';
import { Marca } from '../../core/models/marca.model';
import { LinhaProduto } from '../../core/models/linha-produto.model';

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
  readonly erroExclusao        = signal('');
  readonly buscaMarca          = signal('');
  readonly dropdownMarcaAberto = signal(false);
  readonly marcasFiltradas     = computed(() => {
    const termo = this.buscaMarca().toLowerCase().trim();
    if (!termo) return this.marcas();
    return this.marcas().filter(m => m.nome.toLowerCase().includes(termo));
  });

  readonly linhas              = signal<LinhaProduto[]>([]);
  readonly buscaLinha          = signal('');
  readonly dropdownLinhaAberto = signal(false);
  readonly linhasFiltradas     = computed(() => {
    const termo = this.buscaLinha().toLowerCase().trim();
    if (!termo) return this.linhas();
    return this.linhas().filter(l => l.nome.toLowerCase().includes(termo));
  });

  editandoId: number | null = null;
  form: FormGroup;

  constructor(
    private service: ProdutoService,
    private marcaService: MarcaService,
    private linhaService: LinhaProdutoService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      nome:           ['', [Validators.required, Validators.minLength(2)]],
      idMarca:        [null],
      idLinhaProduto: [null]
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
    this.buscaLinha.set('');
    this.dropdownMarcaAberto.set(false);
    this.dropdownLinhaAberto.set(false);
    this.linhas.set([]);
    this.carregarMarcas();
    this.modalAberto.set(true);
  }

  abrirEditar(item: Produto): void {
    this.editandoId = item.id;
    this.erro.set('');
    this.dropdownMarcaAberto.set(false);
    this.dropdownLinhaAberto.set(false);
    this.carregarMarcas(() => {
      this.form.setValue({ nome: item.nome, idMarca: item.idMarca ?? null, idLinhaProduto: item.idLinhaProduto ?? null });
      this.buscaMarca.set(item.nomeMarca ?? '');
      if (item.idMarca) {
        this.linhaService.listar(undefined, item.idMarca).subscribe(linhas => {
          this.linhas.set(linhas);
          this.buscaLinha.set(item.nomeLinhaProduto ?? '');
        });
      }
    });
    this.modalAberto.set(true);
  }

  selecionarMarca(marca: Marca): void {
    this.form.get('idMarca')!.setValue(marca.id);
    this.buscaMarca.set(marca.nome);
    this.dropdownMarcaAberto.set(false);
    // limpa linha ao trocar de marca
    this.form.get('idLinhaProduto')!.setValue(null);
    this.buscaLinha.set('');
    this.linhaService.listar(undefined, marca.id).subscribe(l => this.linhas.set(l));
  }

  abrirDropdownMarca(): void {
    this.buscaMarca.set('');
    this.form.get('idMarca')!.setValue(null);
    this.form.get('idLinhaProduto')!.setValue(null);
    this.buscaLinha.set('');
    this.linhas.set([]);
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

  selecionarLinha(linha: LinhaProduto): void {
    this.form.get('idLinhaProduto')!.setValue(linha.id);
    this.buscaLinha.set(linha.nome);
    this.dropdownLinhaAberto.set(false);
  }

  limparLinha(): void {
    this.form.get('idLinhaProduto')!.setValue(null);
    this.buscaLinha.set('');
  }

  abrirDropdownLinha(): void {
    this.buscaLinha.set('');
    this.form.get('idLinhaProduto')!.setValue(null);
    this.dropdownLinhaAberto.set(true);
  }

  fecharDropdownLinha(): void {
    setTimeout(() => {
      const idLinha = this.form.get('idLinhaProduto')?.value;
      if (!idLinha) {
        this.buscaLinha.set('');
      } else {
        const linha = this.linhas().find(l => l.id === idLinha);
        this.buscaLinha.set(linha?.nome ?? '');
      }
      this.dropdownLinhaAberto.set(false);
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
    this.dropdownLinhaAberto.set(false);
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.salvando.set(true);
    this.erro.set('');

    const v = this.form.value;
    const req = {
      nome:           v.nome,
      idMarca:        v.idMarca        ? Number(v.idMarca)        : undefined,
      idLinhaProduto: v.idLinhaProduto ? Number(v.idLinhaProduto) : undefined
    };

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
          ? 'Não é possível excluir este produto pois ele está vinculado a uma ou mais despesas.'
          : (err.error?.mensagem ?? 'Erro ao excluir. Tente novamente.');
        this.erroExclusao.set(msg);
      }
    });
  }

  onBusca(event: Event): void {
    const valor = (event.target as HTMLInputElement).value;
    this.carregar(valor || undefined);
  }
}
