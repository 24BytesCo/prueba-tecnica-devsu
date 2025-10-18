import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ClientesService } from './clientes.service';
import { environment } from '../../../environments/environment';

describe('ClientesService (Jest)', () => {
  let service: ClientesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(ClientesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('debería mapear búsqueda de nombre a param "nombres"', () => {
    service.list(1, 10, 'Juan').subscribe();
    const req = http.expectOne(r => r.url === `${environment.apiBaseUrl}/clientes`);
    expect(req.request.params.get('nombres')).toBe('Juan');
    expect(req.request.params.has('numeroDocumento')).toBe(false);
    req.flush({ isSuccess: true, datos: { pagina: 1, tamano: 10, total: 0, items: [] } });
  });

  it('debería mapear búsqueda numérica a param "numeroDocumento"', () => {
    service.list(1, 10, '1002003').subscribe();
    const req = http.expectOne(r => r.url === `${environment.apiBaseUrl}/clientes`);
    expect(req.request.params.get('numeroDocumento')).toBe('1002003');
    expect(req.request.params.has('nombres')).toBe(false);
    req.flush({ isSuccess: true, datos: { pagina: 1, tamano: 10, total: 0, items: [] } });
  });

  it('debería incluir estado cuando se envía', () => {
    service.list(1, 10, '', true).subscribe();
    const req = http.expectOne(r => r.url === `${environment.apiBaseUrl}/clientes`);
    expect(req.request.params.get('estado')).toBe('true');
    req.flush({ isSuccess: true, datos: { pagina: 1, tamano: 10, total: 0, items: [] } });
  });
});

