import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, RouterTestingModule],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'strava-frontend' title`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('strava-frontend');
  });

  it('should render router outlet', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
  });

  describe('Header', () => {
    it('should render the header', () => {
      const fixture = TestBed.createComponent(AppComponent);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      const header = compiled.querySelector('.app-header');
      expect(header).toBeTruthy();
    });

    it('should display the application title "Lukes Strava"', () => {
      const fixture = TestBed.createComponent(AppComponent);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      const title = compiled.querySelector('.app-title h1');
      expect(title?.textContent?.trim()).toBe('Lukes Strava');
    });

    it('should render Activities navigation link', () => {
      const fixture = TestBed.createComponent(AppComponent);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      const activitiesLink = compiled.querySelector('.nav-link');
      expect(activitiesLink?.textContent?.trim()).toBe('Activities');
      expect(activitiesLink?.getAttribute('ng-reflect-router-link')).toBe('/activities');
    });

    it('should render Login button', () => {
      const fixture = TestBed.createComponent(AppComponent);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      const loginButton = compiled.querySelector('.login-button');
      expect(loginButton?.textContent?.trim()).toBe('Login');
      expect(loginButton?.getAttribute('ng-reflect-router-link')).toBe('/login');
    });
  });
});
