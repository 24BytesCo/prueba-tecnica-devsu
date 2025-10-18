// Configuración recomendada por jest-preset-angular (nueva API)
// Evita el warning de deprecación de setup-jest.js
import { setupZoneTestEnv } from 'jest-preset-angular/setup-env/zone';

setupZoneTestEnv();
