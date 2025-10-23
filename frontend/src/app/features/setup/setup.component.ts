import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StravaService } from '../../services/strava.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="login">
      <h2>Login to Strava</h2>
      <p>Sign in with your Strava account to view your running activities.</p>
      <button (click)="login()" class="login-button">
        <span class="strava-icon">🚀</span>
        Login with Strava
      </button>
    </div>
  `,
  styles: [`
    .login { 
      max-width: 480px; 
      margin: 40px auto; 
      display: flex; 
      flex-direction: column; 
      gap: 16px;
      text-align: center;
      padding: 32px;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    }
    .login h2 {
      color: #fc5200;
      margin-bottom: 8px;
    }
    .login p {
      color: #666;
      margin-bottom: 24px;
    }
    .login-button { 
      padding: 12px 24px;
      background-color: #fc5200;
      color: white;
      border: none;
      border-radius: 6px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      transition: background-color 0.2s;
    }
    .login-button:hover {
      background-color: #e04800;
    }
    .strava-icon {
      font-size: 18px;
    }
  `]
})
export class LoginComponent {
  private strava = inject(StravaService);
  private router = inject(Router);

  login() {
    this.strava.getAuthUrl().subscribe(({ url }) => {
      window.location.href = url;
    });
  }
}

