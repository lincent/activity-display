import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { LoginComponent } from './login.component';
import { StravaService } from '../../services/strava.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockStravaService: jasmine.SpyObj<StravaService>;

  beforeEach(async () => {
    // Create spy objects for dependencies
    mockStravaService = jasmine.createSpyObj('StravaService', ['getAuthUrl']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StravaService, useValue: mockStravaService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render login title', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Login to Strava');
  });

  it('should render login button', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector('button.login-button');
    expect(button).toBeTruthy();
    expect(button?.textContent).toContain('Login with Strava');
  });

  it('should have correct description text', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const description = compiled.querySelector('p');
    expect(description?.textContent).toContain('Sign in with your Strava account to view your running activities');
  });

  it('should call StravaService.getAuthUrl when login is called', () => {
    const mockAuthResponse = { url: 'https://www.strava.com/oauth/authorize?test=1' };
    mockStravaService.getAuthUrl.and.returnValue(of(mockAuthResponse));
    spyOn(component, 'navigateToUrl' as any);

    component.login();

    expect(mockStravaService.getAuthUrl).toHaveBeenCalled();
    expect(component['navigateToUrl']).toHaveBeenCalledWith(mockAuthResponse.url);
  });

  it('should handle login button click', () => {
    spyOn(component, 'login');
    fixture.detectChanges();
    
    const button = fixture.nativeElement.querySelector('button.login-button');
    button.click();
    
    expect(component.login).toHaveBeenCalled();
  });

  it('should display strava icon', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.strava-icon');
    expect(icon?.textContent).toBe('🚀');
  });
});