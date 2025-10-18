export interface CuentaListItem {
  cuentaId: string;
  numeroCuenta: string;
  tipoCuentaId: string;
  tipoCuentaNombre: string;
  clienteId: string;
  clienteNombre: string;
  saldoActual: number;
  estado: string; // Activa | Inactiva | Bloqueada
  fechaApertura: string; // ISO
}

export interface CuentaCreateForm {
  numeroCuenta?: string; // opcional: backend puede generar
  tipoCuentaId: string;
  clienteId: string;
  saldoInicial: number;
}

export interface CuentaPutForm {
  numeroCuenta: string;
  tipoCuentaId: string;
  clienteId: string;
}

export interface CuentaEstadoForm {
  estado: string; // Activa | Inactiva | Bloqueada
}

export interface MovimientoItem {
  movimientoId: string;
  fecha: string; // ISO
  tipoCodigo: string; // CR/DB
  monto: number;
  saldoPrevio: number;
  saldoPosterior: number;
  descripcion?: string | null;
}

