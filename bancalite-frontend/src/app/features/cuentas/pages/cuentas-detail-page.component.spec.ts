import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CuentasDetailPageComponent } from './cuentas-detail-page.component';
import { ActivatedRoute, Router } from '@angular/router';
import { CuentasService } from '../../../core/services/cuentas.service';
import { of } from 'rxjs';
import { provideMockStore } from '@ngrx/store/testing';

describe('CuentasDetailPageComponent (Jest)', () => {
  let component: CuentasDetailPageComponent;
  let fixture: ComponentFixture<CuentasDetailPageComponent>;
  let router: Router;

  const mockCuenta = {
    numeroCuenta: '8738-7888-5259',
    tipoCuentaNombre: 'Ahorros',
    clienteNombre: 'María',
    saldoActual: 100,
    estado: 'Activa'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CuentasDetailPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'id-123' } } } },
        { provide: CuentasService, useValue: { get: () => of(mockCuenta), movimientos: () => of([]) } },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } },
        provideMockStore({ initialState: { auth: { loading: false, error: null, profile: null } } })
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(CuentasDetailPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería navegar a /movimientos/nuevo con numeroCuenta como query', () => {
    const spy = jest.spyOn(router, 'navigateByUrl');
    component.nuevoMovimiento();
    expect(spy).toHaveBeenCalledWith(`/movimientos/nuevo?numeroCuenta=${encodeURIComponent(mockCuenta.numeroCuenta)}`);
  });
});
