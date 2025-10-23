import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Location } from '@angular/common';
import { Component } from '@angular/core';

import { HomeComponent } from './home.component';

// Mock component for testing navigation
@Component({
  template: ''
})
class MockActivitiesComponent { }

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HomeComponent, 
        RouterTestingModule.withRoutes([
          { path: 'activities', component: MockActivitiesComponent }
        ])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    location = TestBed.inject(Location);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Hero Section', () => {
    it('should render welcome title', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const title = compiled.querySelector('.hero-section h2');
      expect(title?.textContent?.trim()).toBe('Welcome to Lukes Strava');
    });

    it('should render hero description', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const description = compiled.querySelector('.hero-description');
      expect(description?.textContent?.trim()).toBe(
        'Track and analyze your running activities with this personal Strava dashboard.'
      );
    });

    it('should have centered text alignment', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const heroSection = compiled.querySelector('.hero-section') as HTMLElement;
      const styles = window.getComputedStyle(heroSection);
      expect(styles.textAlign).toBe('center');
    });

    it('should have proper max-width constraint', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const heroSection = compiled.querySelector('.hero-section') as HTMLElement;
      const styles = window.getComputedStyle(heroSection);
      expect(styles.maxWidth).toBe('600px');
    });
  });

  describe('Action Buttons', () => {
    it('should render View Activities button', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const viewActivitiesBtn = compiled.querySelector('.btn-primary');
      expect(viewActivitiesBtn?.textContent?.trim()).toBe('View Activities');
      expect(viewActivitiesBtn?.getAttribute('ng-reflect-router-link')).toBe('/activities');
    });

    it('should have primary button styling', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const primaryBtn = compiled.querySelector('.btn-primary') as HTMLElement;
      const styles = window.getComputedStyle(primaryBtn);
      expect(styles.backgroundColor).toBe('rgb(252, 82, 0)'); // #fc5200
      expect(styles.color).toBe('rgb(255, 255, 255)'); // white in computed style
    });
  });

  describe('Layout and Styling', () => {
    it('should render main content area', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const mainContent = compiled.querySelector('.main-content');
      expect(mainContent).toBeTruthy();
    });
  });

  describe('Navigation Integration', () => {
    it('should have correct routerLink for View Activities button', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const viewActivitiesBtn = compiled.querySelector('.btn-primary') as HTMLElement;
      
      // Test that the routerLink directive is correctly set
      expect(viewActivitiesBtn.getAttribute('ng-reflect-router-link')).toBe('/activities');
    });

    it('should navigate to activities when View Activities button is clicked', async () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const viewActivitiesBtn = compiled.querySelector('.btn-primary') as HTMLElement;
      
      // Click the button which should trigger navigation via RouterLink
      viewActivitiesBtn.click();
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Check that we navigated to the correct route
      expect(location.path()).toBe('/activities');
    });
  });
});