import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Vehicle,TelemetryHistoryItem } from '../models/vehicle.model';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:7051/api/vehicles';

  getVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(this.apiUrl);
  }

  getVehicle(vehicleId: string): Observable<Vehicle> {
    return this.http.get<Vehicle>(
      `${this.apiUrl}/${vehicleId}`
    );
  }

  getHistory(vehicleId: string,limit = 100): Observable<TelemetryHistoryItem[]> {

  return this.http.get<TelemetryHistoryItem[]>(
    `${this.apiUrl}/${vehicleId}/history`,
    {
      params: {
        limit
      }
    }
  );
}
}
