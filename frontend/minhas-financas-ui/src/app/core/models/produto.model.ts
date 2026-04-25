export interface Produto {
  id: number;
  nome: string;
  idMarca: number;
  nomeMarca: string;
}

export interface ProdutoRequest {
  nome: string;
  idMarca: number;
}
