import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DespesaService } from '../../core/services/despesa.service';
import { CategoriaService } from '../../core/services/categoria.service';
import { EstabelecimentoService } from '../../core/services/estabelecimento.service';
import { MarcaService } from '../../core/services/marca.service';
import { ProdutoService } from '../../core/services/produto.service';
import { LinhaProdutoService } from '../../core/services/linha-produto.service';
import { Despesa, FiltrosDespesa } from '../../core/models/despesa.model';
import { Categoria } from '../../core/models/categoria.model';
import { Estabelecimento } from '../../core/models/estabelecimento.model';
import { Marca } from '../../core/models/marca.model';
import { Produto } from '../../core/models/produto.model';
import { LinhaProduto } from '../../core/models/linha-produto.model';

@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './despesas.component.html',
  styleUrl: './despesas.component.scss'
})
export class DespesasComponent implements OnInit {
  readonly lista               = signal<Despesa[]>([]);
  readonly total               = signal(0);
  readonly totalPaginas        = signal(0);
  readonly carregando          = signal(false);
  readonly modalAberto         = signal(false);
  readonly salvando            = signal(false);
  readonly erro                = signal('');
  readonly confirmarExclusaoId = signal<number | null>(null);
  readonly filtrosAbertos      = signal(false);

  readonly categorias       = signal<Categoria[]>([]);
  readonly estabelecimentos = signal<Estabelecimento[]>([]);
  readonly marcas           = signal<Marca[]>([]);
  readonly linhas           = signal<LinhaProduto[]>([]);
  readonly produtos         = signal<Produto[]>([]);

  readonly buscaEstabelecimento                  = signal('');
  readonly dropdownEstabelecimentoAberto         = signal(false);
  readonly modalCadastroEstabelecimentoAberto    = signal(false);
  readonly nomeEstabelecimentoPendente           = signal('');
  readonly cadastrandoEstabelecimento            = signal(false);
  readonly estabelecimentosFiltrados             = computed(() => {
    const termo = this.buscaEstabelecimento().toLowerCase().trim();
    if (!termo) return this.estabelecimentos();
    return this.estabelecimentos().filter(e => e.nome.toLowerCase().includes(termo));
  });

  readonly incluirProduto        = signal(false);
  readonly incluirGranel         = signal(false);
  readonly buscaProduto          = signal('');
  readonly dropdownProdutoAberto = signal(false);
  readonly produtosFiltrados     = computed(() => {
    const termo = this.buscaProduto().toLowerCase().trim();
    if (!termo) return this.produtos();
    return this.produtos().filter(p => p.nome.toLowerCase().includes(termo));
  });

  editandoId: number | null = null;
  paginaAtual = 1;
  readonly tamanhoPagina = 15;
  filtros: FiltrosDespesa = {};

  form: FormGroup;
  filtroForm: FormGroup;

  constructor(
    private service: DespesaService,
    private categoriaService: CategoriaService,
    private estabelecimentoService: EstabelecimentoService,
    private marcaService: MarcaService,
    private produtoService: ProdutoService,
    private linhaService: LinhaProdutoService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      idCategoria:       [null, Validators.required],
      idEstabelecimento: [null, Validators.required],
      idMarca:           [null],
      idLinha:           [null],
      idProduto:         [null],
      descricao:         [''],
      valor:             [null, [Validators.required, Validators.min(0.01)]],
      precoGranel:       [null],
      unidadeGranel:     ['Kg'],
      dataCriacao:       [this.agoraLocal()]
    });

    this.filtroForm = this.fb.group({
      idCategoria:       [null],
      idEstabelecimento: [null],
      idProduto:         [null],
      de:  [''],
      ate: ['']
    });

    this.form.get('idMarca')!.valueChanges.subscribe(idMarca => {
      this.form.get('idLinha')!.setValue(null);
      this.form.get('idProduto')!.setValue(null);
      this.buscaProduto.set('');
      this.linhas.set([]);
      if (idMarca) {
        this.linhaService.listar(undefined, idMarca).subscribe(l => this.linhas.set(l));
      }
      this.carregarProdutos(idMarca ?? undefined);
    });

    this.form.get('idLinha')!.valueChanges.subscribe(idLinha => {
      this.form.get('idProduto')!.setValue(null);
      this.buscaProduto.set('');
      const idMarca = this.form.get('idMarca')?.value ?? undefined;
      this.carregarProdutos(idMarca, idLinha ?? undefined);
    });
  }

  ngOnInit(): void {
    this.carregar();
    this.categoriaService.listar().subscribe(c => this.categorias.set(c));
    this.estabelecimentoService.listar().subscribe(e => this.estabelecimentos.set(e));
    this.marcaService.listar().subscribe(m => this.marcas.set(m));
    this.carregarProdutos();
  }

  private carregarProdutos(idMarca?: number, idLinhaProduto?: number): void {
    this.produtoService.listar(undefined, idMarca, idLinhaProduto).subscribe(p => this.produtos.set(p));
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listar({ ...this.filtros, pagina: this.paginaAtual, tamanhoPagina: this.tamanhoPagina }).subscribe({
      next: res => {
        this.lista.set(res.itens);
        this.total.set(res.total);
        this.totalPaginas.set(res.totalPaginas);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }

  aplicarFiltros(): void {
    const v = this.filtroForm.value;
    this.filtros = {
      idCategoria:       v.idCategoria       || undefined,
      idEstabelecimento: v.idEstabelecimento || undefined,
      idProduto:         v.idProduto         || undefined,
      de:                v.de                || undefined,
      ate:               v.ate               || undefined
    };
    this.paginaAtual = 1;
    this.carregar();
  }

  limparFiltros(): void {
    this.filtroForm.reset();
    this.filtros = {};
    this.paginaAtual = 1;
    this.carregar();
  }

  mudarPagina(pagina: number): void {
    this.paginaAtual = pagina;
    this.carregar();
  }

  get paginas(): number[] {
    return Array.from({ length: this.totalPaginas() }, (_, i) => i + 1);
  }

  toggleGranel(): void {
    const ativar = !this.incluirGranel();
    this.incluirGranel.set(ativar);
    const ctrl = this.form.get('precoGranel')!;
    if (ativar) {
      ctrl.setValidators([Validators.required, Validators.min(0.001)]);
    } else {
      ctrl.clearValidators();
      ctrl.setValue(null);
      ctrl.setErrors(null);
      this.form.get('unidadeGranel')!.setValue('Kg');
    }
    ctrl.updateValueAndValidity();
  }

  toggleProduto(): void {
    const ativar = !this.incluirProduto();
    this.incluirProduto.set(ativar);
    const ctrl = this.form.get('idProduto')!;
    if (ativar) {
      ctrl.setValidators(Validators.required);
    } else {
      ctrl.clearValidators();
      ctrl.setValue(null);
      ctrl.setErrors(null);
      this.form.get('idMarca')!.setValue(null);
      this.buscaProduto.set('');
      this.dropdownProdutoAberto.set(false);
    }
    ctrl.updateValueAndValidity();
  }

  abrirCriar(): void {
    this.editandoId = null;
    this.erro.set('');
    this.incluirProduto.set(false);
    this.incluirGranel.set(false);
    this.linhas.set([]);
    this.form.reset({ dataCriacao: this.agoraLocal(), unidadeGranel: 'Kg' });
    this.form.get('idProduto')!.clearValidators();
    this.form.get('idProduto')!.updateValueAndValidity();
    this.form.get('precoGranel')!.clearValidators();
    this.form.get('precoGranel')!.updateValueAndValidity();
    this.buscaEstabelecimento.set('');
    this.buscaProduto.set('');
    this.dropdownEstabelecimentoAberto.set(false);
    this.dropdownProdutoAberto.set(false);
    this.carregarProdutos();
    this.modalAberto.set(true);
  }

  abrirEditar(item: Despesa): void {
    this.editandoId = item.id;
    this.erro.set('');
    this.dropdownEstabelecimentoAberto.set(false);
    this.dropdownProdutoAberto.set(false);

    const temProduto = !!item.idProduto;
    const temGranel  = !!item.precoGranel;
    this.incluirProduto.set(temProduto);
    this.incluirGranel.set(temGranel);

    const ctrlProduto = this.form.get('idProduto')!;
    if (temProduto) { ctrlProduto.setValidators(Validators.required); }
    else            { ctrlProduto.clearValidators(); }
    ctrlProduto.updateValueAndValidity();

    const ctrlGranel = this.form.get('precoGranel')!;
    if (temGranel) { ctrlGranel.setValidators([Validators.required, Validators.min(0.001)]); }
    else           { ctrlGranel.clearValidators(); }
    ctrlGranel.updateValueAndValidity();

    this.linhas.set([]);
    if (temProduto && item.idMarca) {
      this.linhaService.listar(undefined, item.idMarca).subscribe(l => this.linhas.set(l));
      this.carregarProdutos(item.idMarca, item.idLinhaProduto ?? undefined);
    } else if (temProduto) {
      this.carregarProdutos();
    }

    setTimeout(() => {
      this.form.setValue({
        idCategoria:       item.idCategoria,
        idEstabelecimento: item.idEstabelecimento,
        idMarca:           item.idMarca ?? null,
        idLinha:           item.idLinhaProduto ?? null,
        idProduto:         item.idProduto ?? null,
        descricao:         item.descricao ?? '',
        valor:             item.valor,
        precoGranel:       item.precoGranel ?? null,
        unidadeGranel:     item.unidadeGranel ?? 'Kg',
        dataCriacao:       this.paraLocal(item.dataCriacao)
      });
      this.buscaEstabelecimento.set(item.nomeEstabelecimento);
      this.buscaProduto.set(item.nomeProduto ?? '');
    });

    this.modalAberto.set(true);
  }

  fecharModal(): void {
    this.modalAberto.set(false);
    this.dropdownEstabelecimentoAberto.set(false);
    this.dropdownProdutoAberto.set(false);
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.salvando.set(true);
    this.erro.set('');

    const v = this.form.value;
    const req = {
      idCategoria:       Number(v.idCategoria),
      idEstabelecimento: Number(v.idEstabelecimento),
      idProduto:         v.idProduto ? Number(v.idProduto) : undefined,
      descricao:         v.descricao || undefined,
      valor:             Number(v.valor),
      precoGranel:       this.incluirGranel() && v.precoGranel ? Number(v.precoGranel) : undefined,
      unidadeGranel:     this.incluirGranel() && v.precoGranel ? v.unidadeGranel : undefined,
      dataCriacao:       v.dataCriacao ? new Date(v.dataCriacao).toISOString() : undefined
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

  abrirDropdownEstabelecimento(): void {
    this.buscaEstabelecimento.set('');
    this.form.get('idEstabelecimento')!.setValue(null);
    this.dropdownEstabelecimentoAberto.set(true);
  }

  selecionarEstabelecimento(est: Estabelecimento): void {
    this.form.get('idEstabelecimento')!.setValue(est.id);
    this.buscaEstabelecimento.set(est.nome);
    this.dropdownEstabelecimentoAberto.set(false);
  }

  fecharDropdownEstabelecimento(): void {
    setTimeout(() => {
      this.dropdownEstabelecimentoAberto.set(false);

      const idSelecionado = this.form.get('idEstabelecimento')?.value;
      if (idSelecionado) {
        const est = this.estabelecimentos().find(e => e.id === idSelecionado);
        this.buscaEstabelecimento.set(est?.nome ?? '');
        return;
      }

      const texto = this.buscaEstabelecimento().trim();
      if (!texto) return;

      // texto digitado não corresponde a nenhum item selecionado — oferecer cadastro
      this.nomeEstabelecimentoPendente.set(texto);
      this.modalCadastroEstabelecimentoAberto.set(true);
    }, 150);
  }

  confirmarCadastroEstabelecimento(): void {
    const nome = this.nomeEstabelecimentoPendente();
    this.cadastrandoEstabelecimento.set(true);
    this.estabelecimentoService.criar({ nome }).subscribe({
      next: est => {
        this.estabelecimentos.update(lista => [...lista, est]);
        this.selecionarEstabelecimento(est);
        this.modalCadastroEstabelecimentoAberto.set(false);
        this.cadastrandoEstabelecimento.set(false);
      },
      error: (err: HttpErrorResponse) => {
        // se já existe com este nome, busca na lista e seleciona
        if (err.status === 409) {
          this.estabelecimentoService.listar().subscribe(lista => {
            this.estabelecimentos.set(lista);
            const existente = lista.find(e => e.nome.toLowerCase() === nome.toLowerCase());
            if (existente) this.selecionarEstabelecimento(existente);
          });
        }
        this.modalCadastroEstabelecimentoAberto.set(false);
        this.cadastrandoEstabelecimento.set(false);
      }
    });
  }

  recusarCadastroEstabelecimento(): void {
    this.modalCadastroEstabelecimentoAberto.set(false);
    this.buscaEstabelecimento.set('');
    const ctrl = this.form.get('idEstabelecimento')!;
    ctrl.setValue(null);
    ctrl.markAsTouched();
    ctrl.setErrors({ naoEncontrado: true });
  }

  abrirDropdownProduto(): void {
    this.buscaProduto.set('');
    this.form.get('idProduto')!.setValue(null);
    this.dropdownProdutoAberto.set(true);
  }

  selecionarProduto(prod: Produto): void {
    this.form.get('idProduto')!.setValue(prod.id);
    this.buscaProduto.set(prod.nome);
    this.dropdownProdutoAberto.set(false);
  }

  fecharDropdownProduto(): void {
    setTimeout(() => {
      const id = this.form.get('idProduto')?.value;
      if (!id) {
        this.buscaProduto.set('');
      } else {
        const prod = this.produtos().find(p => p.id === id);
        this.buscaProduto.set(prod?.nome ?? '');
      }
      this.dropdownProdutoAberto.set(false);
    }, 150);
  }

  formatarData(iso: string): string {
    return new Date(iso).toLocaleString('pt-BR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  formatarValor(v: number): string {
    return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  private agoraLocal(): string {
    const d = new Date();
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
    return d.toISOString().slice(0, 16);
  }

  private paraLocal(iso: string): string {
    const d = new Date(iso);
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
    return d.toISOString().slice(0, 16);
  }
}
