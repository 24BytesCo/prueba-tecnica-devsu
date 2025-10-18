import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ClientesListPageComponent } from './clientes-list-page.component';
import { ClientesService } from '../../../core/services/clientes.service';
import { CatalogosService } from '../../../core/services/catalogos.service';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { provideMockStore } from '@ngrx/store/testing';

// Evita que sweetalert2 inyecte CSS en JSDOM durante los tests
jest.mock('sweetalert2', () => ({ __esModule: true, default: { fire: jest.fn() } }));

describe('ClientesListPageComponent (Jest)', () => {
  let component: ClientesListPageComponent;
  let fixture: ComponentFixture<ClientesListPageComponent>;
  let router: Router;

  // Mock simple del servicio con spies configurables
  const listSpy = jest.fn().mockReturnValue(of({ pagina: 1, tamano: 10, total: 0, items: [] }));
  const mockClientes = {
    list: listSpy
  } as Partial<ClientesService> as ClientesService;

  const mockCatalogos = {
    generos: () => of([]),
    tiposDocumento: () => of([])
  } as any;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ClientesListPageComponent],
      providers: [
        { provide: ClientesService, useValue: mockClientes },
        { provide: Router, useValue: { navigateByUrl: jest.fn() } },
        { provide: CatalogosService, useValue: mockCatalogos },
        provideMockStore({ initialState: { auth: { loading: false, error: null, profile: null } } })
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ClientesListPageComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
    // La instancia hace un load() inicial → 1er llamado a list()
    listSpy.mockClear();
  });

  it('debería hacer búsqueda por nombre con debounce', fakeAsync(() => {
    // escribe "Juan" y dispara onSearch
    component['onSearch']({ target: { value: 'Juan' } } as any);
    // avanza el tiempo del debounce (250ms)
    tick(260);
    // expectativa: se llamó a list con q = 'Juan'
    expect(listSpy).toHaveBeenCalled();
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[2]).toBe('Juan');
  }));

  it('debería hacer búsqueda por documento con debounce', fakeAsync(() => {
    component['onSearch']({ target: { value: '1002003' } } as any);
    tick(260);
    expect(listSpy).toHaveBeenCalled();
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[2]).toBe('1002003');
  }));

  it('debería alternar filtro de clientes activos', () => {
    // alterna a false y fuerza load()
    component['toggleSoloActivos']({ target: { checked: false } } as any);
    // expectativa: list llamado con estado false
    const last = listSpy.mock.calls[listSpy.mock.calls.length - 1];
    expect(last[3]).toBe(false);
  });

  it('debería navegar a crear y editar', () => {
    const navSpy = jest.spyOn(router, 'navigateByUrl');
    component.openNew();
    expect(navSpy).toHaveBeenCalledWith('/clientes/nuevo');

    // simula click en editar
    component.openEdit({ clienteId: 'abc' } as any, { preventDefault: () => {} } as any);
    expect(navSpy).toHaveBeenCalledWith('/clientes/abc/editar');
  });
});
