import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MovimientosService } from './movimientos.service';
import { environment } from '../../../environments/environment';

describe('MovimientosService (Jest)', () => {
  let service: MovimientosService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(MovimientosService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('debería enviar Idempotency-Key cuando viene en el payload', () => {
    service.create({ numeroCuenta: '2210', tipoCodigo: 'CRE', monto: 10, idempotencyKey: 'abc-123' }).subscribe();
    const req = http.expectOne(`${environment.apiBaseUrl}/movimientos`);
    expect(req.request.headers.get('Idempotency-Key')).toBe('abc-123');
    req.flush({ isSuccess: true, datos: {} });
  });

  it('debería llamar GET de list con params de fechas y número', () => {
    service.list('2210', '2025-01-01T00:00:00.000Z', '2025-01-02T00:00:00.000Z').subscribe();
    const req = http.expectOne(r => r.url === `${environment.apiBaseUrl}/movimientos`);
    expect(req.request.params.get('numeroCuenta')).toBe('2210');
    expect(req.request.params.get('desde')).toContain('2025-01-01');
    expect(req.request.params.get('hasta')).toContain('2025-01-02');
    req.flush({ isSuccess: true, datos: [] });
  });
});

