export interface ClienteListItem {
  clienteId: string;
  personaId: string;
  nombres: string;
  apellidos: string;
  edad: number;
  tipoDocumentoIdentidadId: string;
  tipoDocumentoIdentidadNombre: string;
  numeroDocumento: string;
  email?: string | null;
  estado: boolean;
}

export interface ClienteForm {
  nombres: string;
  apellidos: string;
  edad: number;
  generoId: string;
  tipoDocumentoIdentidad: string; // el backend espera esta propiedad en create
  numeroDocumento: string;
  direccion?: string | null;
  telefono?: string | null;
  email?: string | null;
  password?: string | null;
}

export interface Paged<T> {
  pagina: number;
  tamano: number;
  total: number;
  items: T[];
}

