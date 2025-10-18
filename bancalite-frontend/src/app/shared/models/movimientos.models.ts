export interface MovimientoCreateForm {
  numeroCuenta: string;
  tipoCodigo: string; // CRE | DEB
  monto: number; // positivo
  idempotencyKey?: string;
  descripcion?: string;
}

export interface MovimientoCreado {
  movimientoId: string;
  cuentaId: string;
  numeroCuenta: string;
  tipoCodigo: string;
  monto: number;
  saldoPrevio: number;
  saldoPosterior: number;
  fecha: string; // ISO
}

