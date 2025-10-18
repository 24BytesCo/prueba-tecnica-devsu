import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginPageComponent } from './login-page.component';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { FormsModule } from '@angular/forms';
import { AuthActions } from '../../../core/state/auth/auth.actions';

describe('LoginPageComponent (Jest)', () => {
  let component: LoginPageComponent;
  let fixture: ComponentFixture<LoginPageComponent>;
  let store: MockStore;
  const initialState = { auth: { loading: false, error: null, profile: null } };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormsModule],
      declarations: [LoginPageComponent],
      providers: [provideMockStore({ initialState })]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(LoginPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería renderizar inputs y botón', () => {
    // inputs visibles
    const inputs = fixture.nativeElement.querySelectorAll('input');
    expect(inputs.length).toBe(2); // email y password
    // botón visible
    const btn = fixture.nativeElement.querySelector('button');
    expect(btn?.textContent).toContain('Entrar');
  });

  it('debería despachar AuthActions.login al hacer click', () => {
    // valores de prueba
    component.email = 'user@test.com';
    component.password = 'secret';
    fixture.detectChanges();

    // espiar dispatch
    const spy = jest.spyOn(store, 'dispatch');

    // click
    const btn = fixture.nativeElement.querySelector('button');
    btn.click();

    // aserción: se despacha la acción con payload correcto
    expect(spy).toHaveBeenCalledWith(AuthActions.login({ email: 'user@test.com', password: 'secret' }));
  });

  it('debería mostrar mensaje cuando hay error (401)', () => {
    // forzar error en el store
    store.setState({ auth: { loading: false, error: 'Credenciales inválidas', profile: null } });
    store.refreshState();
    fixture.detectChanges();

    // el mensaje debe estar visible en la plantilla
    const msg = fixture.nativeElement.querySelector('.error');
    expect(msg?.textContent).toContain('Credenciales inválidas');
  });
});

