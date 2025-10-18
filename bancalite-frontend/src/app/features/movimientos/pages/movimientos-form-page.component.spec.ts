import { ComponentFixture, TestBed, fakeAsync, flushMicrotasks } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MovimientosFormPageComponent } from './movimientos-form-page.component';
import { CatalogosService } from '../../../core/services/catalogos.service';
import { MovimientosService } from '../../../core/services/movimientos.service';
import { CuentasService } from '../../../core/services/cuentas.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

// Mock de SweetAlert2 para evitar CSS injection y permitir asserts de error
jest.mock('sweetalert2', () => ({ __esModule: true, default: { fire: jest.fn() } }));
import Swal from 'sweetalert2';

describe('MovimientosFormPageComponent (Jest)', () => {
  let component: MovimientosFormPageComponent;
  let fixture: ComponentFixture<MovimientosFormPageComponent>;
  let router: Router;
  const createSpy = jest.fn().mockReturnValue(of({ isSuccess: true, datos: { saldoPrevio: 10, saldoPosterior: 20 } }));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      declarations: [MovimientosFormPageComponent],
      providers: [
        { provide: CatalogosService, useValue: { tiposMovimiento: () => of([{ id: 'x', codigo: 'CRE', nombre: 'Crédito', activo: true }]) } },
        { provide: MovimientosService, useValue: { create: createSpy } },
        { provide: CuentasService, useValue: { list: () => of({ pagina: 1, tamano: 5, total: 0, items: [] }) } },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { get: () => '2210' } } } },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(MovimientosFormPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería redirigir a /movimientos con query después de crear', fakeAsync(() => {
    component.form.patchValue({ numeroCuenta: '2210', tipoCodigo: 'CRE', monto: 10, descripcion: '' });
    const navSpy = jest.spyOn(router, 'navigateByUrl');
    // el SweetAlert de éxito debe resolver una promesa para que se ejecute el then()
    const fireMock = (Swal as any).fire as jest.Mock;
    fireMock.mockResolvedValue({});
    component.save();
    // resuelve microtareas (promesas) encadenadas al Swal
    flushMicrotasks();
    expect(createSpy).toHaveBeenCalled();
    expect(navSpy).toHaveBeenCalledWith('/movimientos?numeroCuenta=2210');
  }));

  it('debería mostrar mensaje en error (saldo no disponible / tope diario)', () => {
    const svc = TestBed.inject(MovimientosService) as any;
    const fireMock = (Swal as any).fire as jest.Mock;
    fireMock.mockClear();
    svc.create = jest.fn().mockReturnValueOnce(throwError(() => ({ error: { title: 'Unprocessable: Saldo no disponible' } })));
    component.form.patchValue({ numeroCuenta: '2210', tipoCodigo: 'DEB', monto: 999 });
    component.save();
    expect(fireMock).toHaveBeenCalled();
    const [[args]] = fireMock.mock.calls;
    expect(args.title).toContain('No se pudo registrar');
  });
});
