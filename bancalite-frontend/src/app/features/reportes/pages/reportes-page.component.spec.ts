import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReportesPageComponent } from './reportes-page.component';
import { ReportesService } from '../../../core/services/reportes.service';
import { CuentasService } from '../../../core/services/cuentas.service';
import { ClientesService } from '../../../core/services/clientes.service';
import { of } from 'rxjs';

describe('ReportesPageComponent (Jest)', () => {
  let component: ReportesPageComponent;
  let fixture: ComponentFixture<ReportesPageComponent>;

  const estadoMock = {
    clienteNombre: 'Juan Pérez',
    numeroCuenta: '2210',
    desde: '2025-01-01T00:00:00.000Z',
    hasta: '2025-01-02T00:00:00.000Z',
    totalCreditos: 100,
    totalDebitos: 50,
    saldoInicial: 200,
    saldoFinal: 250,
    movimientos: [
      { fecha: '2025-01-01T10:00:00.000Z', numeroCuenta: '2210', tipoCodigo: 'CRE', monto: 100, saldoPrevio: 200, saldoPosterior: 300, descripcion: 'Depósito' },
      { fecha: '2025-01-01T12:00:00.000Z', numeroCuenta: '2210', tipoCodigo: 'DEB', monto: 50, saldoPrevio: 300, saldoPosterior: 250, descripcion: 'Retiro' }
    ]
  } as any;

  const repSvc = {
    estadoCuenta: jest.fn().mockReturnValue(of(estadoMock)),
    estadoCuentaPdfBase64: jest.fn().mockReturnValue(of({ fileName: 'rep.pdf', contentType: 'application/pdf', base64: btoa('PDF') })),
    estadoCuentaJsonBlob: jest.fn().mockReturnValue(of(new Blob(['{"ok":true}'], { type: 'application/json' })))
  } as Partial<ReportesService> as ReportesService;

  const ctasSvc = {
    list: jest.fn().mockReturnValue(of({ pagina: 1, tamano: 5, total: 1, items: [{ numeroCuenta: '2210', clienteNombre: 'Juan Pérez', tipoCuentaNombre: 'Ahorros' }] }))
  } as Partial<CuentasService> as CuentasService;

  const cliSvc = {
    list: jest.fn().mockReturnValue(of({ pagina: 1, tamano: 5, total: 1, items: [{ clienteId: 'cli1', nombres: 'Juan', apellidos: 'Pérez', numeroDocumento: '1002003001' }] }))
  } as Partial<ClientesService> as ClientesService;

  beforeEach(async () => {
    // Mocks de URL y link para descargas
    if (!(URL as any).createObjectURL) {
      Object.defineProperty(URL, 'createObjectURL', { writable: true, value: jest.fn(() => 'blob://mock') });
    } else {
      (URL.createObjectURL as any) = jest.fn(() => 'blob://mock');
    }
    const realCreate = (Document.prototype as any).createElement as (this: Document, tagName: string) => any;
    jest.spyOn(document, 'createElement').mockImplementation(((tag: string) => {
      if (tag === 'a') {
        return { click: jest.fn(), set href(v: string) {}, get href() { return 'blob://mock'; }, set download(v: string) {}, style: {} } as any;
      }
      return realCreate.call(document, tag);
    }) as any);

    await TestBed.configureTestingModule({
      declarations: [ReportesPageComponent],
      providers: [
        { provide: ReportesService, useValue: repSvc },
        { provide: CuentasService, useValue: ctasSvc },
        { provide: ClientesService, useValue: cliSvc }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ReportesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería sugerir cuentas y consultar por cuenta', fakeAsync(() => {
    component.setModo('cuenta');
    component.onBuscarCuenta({ target: { value: '2210' } } as any);
    tick(260);
    expect(ctasSvc.list).toHaveBeenCalled();

    // seleccionar sugerencia
    component.seleccionarCuenta({ numeroCuenta: '2210' } as any);
    component.consultar();
    expect(repSvc.estadoCuenta).toHaveBeenCalledWith(expect.objectContaining({ numeroCuenta: '2210' }));
    // datos en pantalla
    expect(component.data?.numeroCuenta).toBe('2210');
  }));

  it('debería sugerir clientes y consultar por cliente', fakeAsync(() => {
    component.setModo('cliente');
    component.onBuscarCliente({ target: { value: 'Juan' } } as any);
    tick(260);
    expect(cliSvc.list).toHaveBeenCalled();
    // seleccionar cliente
    component.seleccionarCliente({ clienteId: 'cli1', nombres: 'Juan', apellidos: 'Pérez', numeroDocumento: '1002003001' } as any);
    component.consultar();
    expect(repSvc.estadoCuenta).toHaveBeenCalledWith(expect.objectContaining({ clienteId: 'cli1' }));
  }));

  it('debería descargar JSON', () => {
    component.modo = 'cuenta';
    component.numeroCuenta = '2210';
    component.descargarJson();
    expect(repSvc.estadoCuentaJsonBlob).toHaveBeenCalled();
  });

  it('debería descargar PDF (Base64)', () => {
    component.modo = 'cuenta';
    component.numeroCuenta = '2210';
    component.descargarPdfBase64();
    expect(repSvc.estadoCuentaPdfBase64).toHaveBeenCalled();
  });
});
