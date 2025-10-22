import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StravaService, RunDto } from '../../services/strava.service';

function formatDuration(totalSeconds: number): string {
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  const s = totalSeconds % 60;
  return [h, m, s]
    .map((v, i) => i === 0 ? v.toString() : v.toString().padStart(2, '0'))
    .join(':');
}

@Component({
  selector: 'app-activities-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="wrap">
      <h2>Latest Runs</h2>
      <div *ngIf="loading()">Loading…</div>
      <div *ngIf="error()" class="error">{{ error() }}</div>
      <div class="list" *ngIf="!loading() && runs().length">
        <div class="card" *ngFor="let run of runs()">
          <div class="title">{{ run.name }}</div>
          <div class="meta">
            <span>{{ run.startDateLocal | date:'mediumDate' }}</span>
            <span>{{ run.distanceKm | number:'1.2-2' }} km</span>
            <span>{{ formatPace(run.averagePaceMinPerKm) }} /km</span>
            <span>{{ formatDuration(run.movingTimeSec) }}</span>
          </div>
        </div>
      </div>
      <div *ngIf="!loading() && !runs().length">No runs found. Try <a routerLink="/setup">connecting Strava</a>.</div>
    </div>
  `,
  styles: [`
    .wrap { max-width: 720px; margin: 24px auto; padding: 0 12px; }
    .list { display: grid; gap: 12px; }
    .card { border: 1px solid #ddd; border-radius: 8px; padding: 12px; }
    .title { font-weight: 600; margin-bottom: 4px; }
    .meta { display: flex; gap: 12px; color: #555; font-size: 0.9rem; flex-wrap: wrap; }
    .error { color: #b00020; }
  `]
})
export class ActivitiesListComponent {
  private strava = inject(StravaService);
  runs = signal<RunDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor() {
    this.strava.getLatestRuns(10).subscribe({
      next: runs => { this.runs.set(runs); this.loading.set(false); },
      error: err => { this.error.set('Failed to load runs'); this.loading.set(false); }
    });
  }

  formatDuration = formatDuration;
  formatPace(pace: number) {
    const min = Math.floor(pace);
    const sec = Math.round((pace - min) * 60);
    return `${min}:${sec.toString().padStart(2, '0')}`;
  }
}

