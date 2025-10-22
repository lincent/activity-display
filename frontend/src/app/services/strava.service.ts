import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

const API_BASE_URL = 'http://localhost:5000';

export interface RunDto {
  id: number;
  name: string;
  startDateLocal: string;
  distanceKm: number;
  movingTimeSec: number;
  averagePaceMinPerKm: number;
}

@Injectable({ providedIn: 'root' })
export class StravaService {
  private http = inject(HttpClient);

  getAuthUrl() {
    return this.http.get<{ url: string }>(`${API_BASE_URL}/api/strava/auth-url`);
  }

  getLatestRuns(perPage = 10) {
    return this.http.get<RunDto[]>(`${API_BASE_URL}/api/strava/activities`, { params: { perPage } as any });
  }
}

