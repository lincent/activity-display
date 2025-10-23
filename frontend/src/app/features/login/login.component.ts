import { Component, inject } from '@angular/core';
import { CommonModule, DOCUMENT } from '@angular/common';
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
  private document = inject(DOCUMENT);

  login() {
    this.strava.getAuthUrl().subscribe(({ url }) => {
      this.navigateToUrl(url);
    });
  }

  protected navigateToUrl(url: string): void {
    this.document.location.href = url;
  }
}