export interface LinhaProduto {
  id: number;
  nome: string;
  idMarca: number;
  nomeMarca: string;
}

export interface LinhaProdutoRequest {
  nome: string;
  idMarca: number;
}
