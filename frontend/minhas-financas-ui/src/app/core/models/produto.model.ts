export interface Produto {
  id: number;
  nome: string;
  idMarca?: number;
  nomeMarca?: string;
  idLinhaProduto?: number;
  nomeLinhaProduto?: string;
}

export interface ProdutoRequest {
  nome: string;
  idMarca?: number;
  idLinhaProduto?: number;
}
