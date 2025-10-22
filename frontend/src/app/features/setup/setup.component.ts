import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StravaService } from '../../services/strava.service';

@Component({
  selector: 'app-setup',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="setup">
      <h2>Connect Strava</h2>
      <p>One-time authorization to fetch your activities.</p>
      <button (click)="connect()">Connect Strava</button>
    </div>
  `,
  styles: [`
    .setup { max-width: 480px; margin: 40px auto; display: flex; flex-direction: column; gap: 12px; }
    button { padding: 10px 16px; }
  `]
})
export class SetupComponent {
  private strava = inject(StravaService);
  private router = inject(Router);

  connect() {
    this.strava.getAuthUrl().subscribe(({ url }) => {
      window.location.href = url;
    });
  }
}

