export interface Despesa {
  id: number;
  idCategoria: number;
  nomeCategoria: string;
  idEstabelecimento: number;
  nomeEstabelecimento: string;
  idProduto?: number;
  nomeProduto?: string;
  idMarca?: number;
  nomeMarca?: string;
  idLinhaProduto?: number;
  nomeLinhaProduto?: string;
  descricao?: string;
  valor: number;
  precoGranel?: number;
  unidadeGranel?: string;
  dataCriacao: string;
}

export interface DespesaRequest {
  idCategoria: number;
  idEstabelecimento: number;
  idProduto?: number;
  descricao?: string;
  valor: number;
  precoGranel?: number;
  unidadeGranel?: string;
  dataCriacao?: string;
}

export interface ResultadoPaginado<T> {
  itens: T[];
  total: number;
  pagina: number;
  totalPaginas: number;
}

export interface FiltrosDespesa {
  idCategoria?: number;
  idEstabelecimento?: number;
  idProduto?: number;
  de?: string;
  ate?: string;
  pagina?: number;
  tamanhoPagina?: number;
}

export interface Dashboard {
  resumo: ResumoDespesas;
  evolucaoMensal: EvolucaoMensalItem[];
  topEstabelecimentos: RankingItem[];
  topProdutos: RankingItem[];
}

export interface ResumoDespesas {
  totalEssencial: number;
  totalLazer: number;
  totalInvestimento: number;
  totalGeral: number;
}

export interface EvolucaoMensalItem {
  ano: number;
  mes: number;
  totalEssencial: number;
  totalLazer: number;
  totalInvestimento: number;
}

export interface RankingItem {
  nome: string;
  detalhe?: string;
  total: number;
}
