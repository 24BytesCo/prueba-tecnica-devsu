import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CuentasListPageComponent } from './cuentas-list-page.component';
import { CuentasService } from '../../../core/services/cuentas.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';

// Evita inyección de CSS de SweetAlert en JSDOM
jest.mock('sweetalert2', () => ({ __esModule: true, default: { fire: jest.fn() } }));

describe('CuentasListPageComponent (Jest)', () => {
  let component: CuentasListPageComponent;
  let fixture: ComponentFixture<CuentasListPageComponent>;

  const listSpy = jest.fn().mockReturnValue(of({ pagina: 1, tamano: 10, total: 0, items: [] }));
  const mockCuentas = { list: listSpy } as Partial<CuentasService> as CuentasService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CuentasListPageComponent],
      providers: [
        { provide: CuentasService, useValue: mockCuentas },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CuentasListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    listSpy.mockClear();
  });

  it('debería buscar con debounce por texto', fakeAsync(() => {
    component['onSearch']({ target: { value: '2210' } } as any);
    tick(260);
    expect(listSpy).toHaveBeenCalled();
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[2]).toEqual(expect.objectContaining({ q: '2210' }));
  }));

  it('debería aplicar filtro de cuentas activas', () => {
    component['toggleActivo']({ target: { checked: false } } as any);
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[2]).toEqual(expect.objectContaining({ activo: false }));
  });

  it('debería aplicar filtro de clientes activos', () => {
    component['toggleClientesActivos']({ target: { checked: false } } as any);
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[2]).toEqual(expect.objectContaining({ clientesActivos: false }));
  });
});

