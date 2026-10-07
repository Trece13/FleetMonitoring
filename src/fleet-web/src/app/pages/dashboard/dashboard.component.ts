import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnDestroy
} from '@angular/core';

import { CommonModule } from '@angular/common';

import * as L from 'leaflet';

import {
  TelemetryHistoryItem,
  Vehicle,
  VehicleStateUpdated
} from '../../models/vehicle.model';

import {
  VehicleService
} from '../../services/vehicle.service';

import {
  RealtimeService
} from '../../services/realtime.service';


@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent
  implements AfterViewInit, OnDestroy {

  vehicles: Vehicle[] = [];

  selectedVehicleId?: string;

  private map?: L.Map;

  private readonly markers =
    new Map<string, L.CircleMarker>();

  private historyPolyline?: L.Polyline;


  constructor(
    private readonly vehicleService: VehicleService,
    private readonly realtimeService: RealtimeService,
    private readonly cdr: ChangeDetectorRef
  ) {
  }


  // =========================================================
  // LIFECYCLE
  // =========================================================

  ngAfterViewInit(): void {

    // Primero creamos el mapa.
    this.initializeMap();

    // Después cargamos vehículos.
    this.loadVehicles();

    // Finalmente conectamos SignalR.
    this.realtimeService.start(
  state =>
    this.handleRealtimeUpdate(state),

  vehicleId =>
    this.handleVehicleDeleted(vehicleId)
);

    // Leaflet a veces necesita recalcular el tamaño
    // después de que Angular termina de renderizar.
    setTimeout(() => {
      this.map?.invalidateSize();
    }, 0);
  }


  ngOnDestroy(): void {

    this.realtimeService.stop();

    this.map?.remove();
  }


  // =========================================================
  // MAP
  // =========================================================

  private initializeMap(): void {

    this.map = L.map('fleet-map')
      .setView(
        [4.6500, -74.0600],
        14
      );

    L.tileLayer(
      'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
      {
        maxZoom: 19,
        attribution:
          '&copy; OpenStreetMap contributors'
      }
    ).addTo(this.map);
  }


  // =========================================================
  // VEHICLES
  // =========================================================

  private loadVehicles(): void {

    this.vehicleService
      .getVehicles()
      .subscribe({

        next: vehicles => {

          this.vehicles = vehicles;

          for (const vehicle of vehicles) {
            this.updateMarker(vehicle);
          }

          this.cdr.markForCheck();
        },

        error: error => {

          console.error(
            'Could not load vehicles',
            error
          );
        }
      });
  }


  // =========================================================
  // REALTIME
  // =========================================================

  private handleRealtimeUpdate(
    state: VehicleStateUpdated
  ): void {

    const vehicle =
      this.vehicles.find(
        x => x.vehicleId === state.vehicleId
      );

    if (vehicle) {

      vehicle.latitude =
        state.latitude;

      vehicle.longitude =
        state.longitude;

      vehicle.status =
        state.status;

      vehicle.lastSeenAtUtc =
        state.lastSeenAtUtc;

      this.updateMarker(vehicle);

    } else {

      const newVehicle: Vehicle = {
        id: '',
        vehicleId: state.vehicleId,
        name: state.vehicleId,
        latitude: state.latitude,
        longitude: state.longitude,
        status: state.status,
        lastSeenAtUtc: state.lastSeenAtUtc
      };

      this.vehicles = [
        ...this.vehicles,
        newVehicle
      ];

      this.updateMarker(newVehicle);
    }


    // Si estamos viendo el histórico de este vehículo,
    // agregamos la nueva posición a la ruta.
    if (
      this.selectedVehicleId === state.vehicleId &&
      this.historyPolyline
    ) {

      this.historyPolyline.addLatLng([
        state.latitude,
        state.longitude
      ]);
    }


    this.cdr.markForCheck();
  }


  // =========================================================
  // MARKERS
  // =========================================================

  private updateMarker(
    vehicle: Vehicle
  ): void {

    if (
      !this.map ||
      vehicle.latitude === null ||
      vehicle.longitude === null
    ) {
      return;
    }


    const position: L.LatLngExpression = [
      vehicle.latitude,
      vehicle.longitude
    ];


    const existingMarker =
      this.markers.get(
        vehicle.vehicleId
      );


    // Si ya existe el marcador,
    // simplemente lo movemos.
    if (existingMarker) {

      existingMarker.setLatLng(
        position
      );

      existingMarker.bindPopup(
        this.buildPopup(vehicle)
      );

      return;
    }


    // Si no existe, creamos uno nuevo.
    const marker =
      L.circleMarker(
        position,
        {
          radius: 9,
          weight: 3,
          fillOpacity: 0.8
        }
      );


    marker.bindPopup(
      this.buildPopup(vehicle)
    );


    marker.on(
      'click',
      () => this.selectVehicle(vehicle)
    );


    marker.addTo(
      this.map
    );


    this.markers.set(
      vehicle.vehicleId,
      marker
    );
  }


  private buildPopup(
    vehicle: Vehicle
  ): string {

    return `
      <strong>${vehicle.vehicleId}</strong>
      <br>
      Estado: ${vehicle.status}
      <br>
      Lat: ${vehicle.latitude}
      <br>
      Lng: ${vehicle.longitude}
    `;
  }


  // =========================================================
  // COUNTERS
  // =========================================================

  get movingCount(): number {

    return this.vehicles.filter(
      x => x.status === 'Moving'
    ).length;
  }


  get stationaryCount(): number {

    return this.vehicles.filter(
      x => x.status === 'Stationary'
    ).length;
  }


  get stoppedCount(): number {

    return this.vehicles.filter(
      x => x.status === 'Stopped'
    ).length;
  }


  // =========================================================
  // VEHICLE SELECTION
  // =========================================================

  selectVehicle(
    vehicle: Vehicle
  ): void {

    this.selectedVehicleId =
      vehicle.vehicleId;


    if (
      vehicle.latitude !== null &&
      vehicle.longitude !== null
    ) {

      this.map?.flyTo(
        [
          vehicle.latitude,
          vehicle.longitude
        ],
        16
      );
    }


    this.loadHistory(
      vehicle.vehicleId
    );
  }


  // =========================================================
  // HISTORY
  // =========================================================

  private loadHistory(
    vehicleId: string
  ): void {

    this.vehicleService
      .getHistory(
        vehicleId,
        100
      )
      .subscribe({

        next: history => {

          this.drawHistory(
            history
          );
        },

        error: error => {

          console.error(
            'Could not load vehicle history',
            error
          );
        }
      });
  }


  private drawHistory(
    history: TelemetryHistoryItem[]
  ): void {

    if (!this.map) {
      return;
    }


    // Quitamos la ruta anterior.
    if (this.historyPolyline) {

      this.historyPolyline.remove();

      this.historyPolyline =
        undefined;
    }


    if (history.length === 0) {
      return;
    }


    // Mongo devuelve:
    //
    // newest -> oldest
    //
    // Leaflet necesita:
    //
    // oldest -> newest

    const orderedHistory = [
      ...history
    ].reverse();


    const positions:
      L.LatLngExpression[] =
      orderedHistory.map(
        item => [
          item.latitude,
          item.longitude
        ]
      );


    this.historyPolyline =
      L.polyline(
        positions,
        {
          weight: 4,
          opacity: 0.8
        }
      )
      .addTo(this.map);


    // Ajustamos el mapa para mostrar
    // toda la ruta.
    if (positions.length > 1) {

      this.map.fitBounds(
        this.historyPolyline.getBounds(),
        {
          padding: [40, 40]
        }
      );
    }
  }

  private handleVehicleDeleted(
  vehicleId: string
): void {

  // 1. Quitamos el vehículo de la lista.
  this.vehicles =
    this.vehicles.filter(
      x => x.vehicleId !== vehicleId
    );


  // 2. Quitamos su marcador del mapa.
  const marker =
    this.markers.get(vehicleId);

  if (marker) {

    marker.remove();

    this.markers.delete(
      vehicleId
    );
  }


  // 3. Si era el vehículo seleccionado,
  // quitamos también su histórico.
  if (
    this.selectedVehicleId === vehicleId
  ) {

    this.selectedVehicleId =
      undefined;

    if (this.historyPolyline) {

      this.historyPolyline.remove();

      this.historyPolyline =
        undefined;
    }
  }


  this.cdr.markForCheck();
}
}