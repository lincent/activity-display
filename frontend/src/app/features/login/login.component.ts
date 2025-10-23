import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StravaService } from '../../services/strava.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
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