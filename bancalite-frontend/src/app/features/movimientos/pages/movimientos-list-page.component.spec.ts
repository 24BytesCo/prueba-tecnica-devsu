import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { MovimientosListPageComponent } from './movimientos-list-page.component';
import { MovimientosService } from '../../../core/services/movimientos.service';
import { CuentasService } from '../../../core/services/cuentas.service';
import { CatalogosService } from '../../../core/services/catalogos.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { provideMockStore } from '@ngrx/store/testing';

jest.mock('sweetalert2', () => ({ __esModule: true, default: { fire: jest.fn() } }));

describe('MovimientosListPageComponent (Jest)', () => {
  let component: MovimientosListPageComponent;
  let fixture: ComponentFixture<MovimientosListPageComponent>;

  const listMovsSpy = jest.fn().mockReturnValue(of([]));
  const listCtasSpy = jest.fn().mockReturnValue(of({ pagina: 1, tamano: 5, total: 1, items: [
    { numeroCuenta: '2210', clienteNombre: 'Juan', tipoCuentaNombre: 'Ahorros' }
  ] }));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MovimientosListPageComponent],
      providers: [
        { provide: MovimientosService, useValue: { list: listMovsSpy } },
        { provide: CuentasService, useValue: { list: listCtasSpy } },
        { provide: CatalogosService, useValue: { tiposMovimiento: () => of([]) } },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { get: () => null } } } },
        provideMockStore({ initialState: { auth: { loading: false, error: null, profile: null } } })
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MovimientosListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    listMovsSpy.mockClear();
  });

  it('debería sugerir cuentas al teclear y listar al seleccionar', fakeAsync(() => {
    // escribe para sugerencias
    component.onSearchCuenta({ target: { value: 'Juan' } } as any);
    tick(260);
    expect(listCtasSpy).toHaveBeenCalled();

    // seleccionar primera sugerencia → setea numeroCuenta y carga lista
    component.seleccionarCuenta({ numeroCuenta: '2210' } as any);
    // load() se dispara por Subject con debounce → avanzamos tiempo
    tick(260);
    expect(component.numeroCuenta).toBe('2210');
    expect(listMovsSpy).toHaveBeenCalled();
  }));

  it('debería recargar inmediatamente al cambiar fechas', () => {
    component.numeroCuenta = '2210';
    component.onDesdeChange({ target: { value: '2025-01-01' } } as any);
    component.onHastaChange({ target: { value: '2025-01-02' } } as any);
    expect(listMovsSpy).toHaveBeenCalledTimes(2);
  });
});
