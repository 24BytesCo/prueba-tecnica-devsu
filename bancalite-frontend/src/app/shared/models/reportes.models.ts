export interface EstadoCuentaItemDto {
  fecha: string;
  numeroCuenta: string;
  tipoCodigo: string;
  monto: number;
  saldoPrevio: number;
  saldoPosterior: number;
  descripcion?: string | null;
}

export interface EstadoCuentaDto {
  clienteId?: string | null;
  clienteNombre?: string | null;
  numeroCuenta?: string | null;
  desde: string;
  hasta: string;
  saldoInicial: number;
  saldoFinal: number;
  totalDebitos: number;
  totalCreditos: number;
  movimientos: EstadoCuentaItemDto[];
}

