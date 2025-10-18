import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HeaderComponent } from './header.component';
import { provideMockStore, MockStore } from '@ngrx/store/testing';

describe('HeaderComponent (Jest)', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;
  let store: MockStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [HeaderComponent],
      providers: [provideMockStore({ initialState: { auth: { loading: false, error: null, profile: null } } })]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería mostrar el email en el header cuando hay perfil', () => {
    // simula login success
    store.setState({ auth: { loading: false, error: null, profile: { email: 'user@test.com' } } });
    store.refreshState();
    fixture.detectChanges();

    // texto visible en el header
    const el = fixture.nativeElement.querySelector('.user');
    expect(el?.textContent).toContain('user@test.com');
  });
});

