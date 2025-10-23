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
  templateUrl: './activities-list.component.html',
  styleUrls: ['./activities-list.component.css']
})
export class ActivitiesListComponent {
  runs = signal<RunDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  strava = inject(StravaService);

  constructor() {
    this.strava.getLatestRuns(10).subscribe({
      next: runs => { this.runs.set(runs); this.loading.set(false); },
      error: () => { this.error.set('Failed to load runs'); this.loading.set(false); }
    });
  }

  formatDuration = formatDuration;
  formatPace(pace: number) {
    const min = Math.floor(pace);
    const sec = Math.round((pace - min) * 60);
    return `${min}:${sec.toString().padStart(2, '0')}`;
  }
}

