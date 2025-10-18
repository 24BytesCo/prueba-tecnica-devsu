import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { CuentasFormPageComponent } from './cuentas-form-page.component';
import { CuentasService } from '../../../core/services/cuentas.service';
import { CatalogosService } from '../../../core/services/catalogos.service';
import { ClientesService } from '../../../core/services/clientes.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';

describe('CuentasFormPageComponent (Jest)', () => {
  let component: CuentasFormPageComponent;
  let fixture: ComponentFixture<CuentasFormPageComponent>;
  let router: Router;

  const createSpy = jest.fn().mockReturnValue(of({ isSuccess: true, datos: {} }));
  const mockCuentas = {
    create: createSpy
  } as Partial<CuentasService> as CuentasService;

  const mockCatalogos = {
    tiposCuenta: () => of([{ id: 't1', codigo: 'AHO', nombre: 'Ahorros', activo: true }])
  } as Partial<CatalogosService> as CatalogosService;

  const listClientesSpy = jest.fn().mockReturnValue(
    of({ pagina: 1, tamano: 5, total: 1, items: [{ clienteId: 'cli1', nombres: 'Juan', apellidos: 'Pérez', numeroDocumento: '1002003001' }] })
  );
  const mockClientes = { list: listClientesSpy } as Partial<ClientesService> as ClientesService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      declarations: [CuentasFormPageComponent],
      providers: [
        { provide: CuentasService, useValue: mockCuentas },
        { provide: CatalogosService, useValue: mockCatalogos },
        { provide: ClientesService, useValue: mockClientes },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: (_: string) => null } } } },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(CuentasFormPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería mostrar sugerencias y fijar cliente al seleccionar', fakeAsync(() => {
    // dispara búsqueda de cliente (sujeta a debounce de 250ms)
    component.onSearchCliente({ target: { value: 'Juan' } } as any);
    tick(260);
    // la lista de sugerencias se llena
    expect(component['sugerencias'].length).toBeGreaterThan(0);
    // seleccionar la primera opción
    const sel = component['sugerencias'][0];
    component.seleccionarCliente(sel);
    // el formulario debe contener el clienteId y el texto de clienteNombre
    expect(component.form.get('clienteId')?.value).toBe('cli1');
    expect(component.form.get('clienteNombre')?.value).toContain('Juan');
    // y se limpian las sugerencias
    expect(component['sugerencias'].length).toBe(0);
  }));

  it('debería crear cuenta y navegar a /cuentas', () => {
    // completa datos mínimos
    component.form.patchValue({ tipoCuentaId: 't1', clienteId: 'cli1', saldoInicial: 200 });
    const navSpy = jest.spyOn(router, 'navigateByUrl');
    component.save();
    expect(createSpy).toHaveBeenCalledWith({ numeroCuenta: '', tipoCuentaId: 't1', clienteId: 'cli1', saldoInicial: 200 });
    expect(navSpy).toHaveBeenCalledWith('/cuentas');
  });
});

