import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { LayoutModule } from './layout/layout.module';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { authReducer } from './core/state/auth/auth.reducer';
import { AuthEffects } from './core/state/auth/auth.effects';
import { environment } from '../environments/environment';
import { hydrationMetaReducer } from './core/state/hydration.meta-reducer';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    CoreModule,
    LayoutModule,
    // Wiring básico de NgRx: store raíz + efectos + devtools
    StoreModule.forRoot({ auth: authReducer }, { metaReducers: [hydrationMetaReducer] }),
    EffectsModule.forRoot([AuthEffects]),
    StoreDevtoolsModule.instrument({ maxAge: 25, logOnly: environment.production }),
    AppRoutingModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
