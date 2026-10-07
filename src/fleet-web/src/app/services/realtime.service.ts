import { Injectable } from '@angular/core';

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel
} from '@microsoft/signalr';

import {
  VehicleStateUpdated
} from '../models/vehicle.model';


@Injectable({
  providedIn: 'root'
})
export class RealtimeService {

  private hubConnection?: HubConnection;


  async start(
    onVehicleUpdated: (
      state: VehicleStateUpdated
    ) => void,

    onVehicleDeleted: (
      vehicleId: string
    ) => void
  ): Promise<void> {

    if (this.hubConnection) {
      return;
    }


    this.hubConnection =
      new HubConnectionBuilder()
        .withUrl(
          'https://localhost:7051/hubs/fleet'
        )
        .withAutomaticReconnect()
        .configureLogging(
          LogLevel.Information
        )
        .build();


    // Posición / estado actualizado
    this.hubConnection.on(
      'VehicleStateUpdated',
      (
        state: VehicleStateUpdated
      ) => {
        onVehicleUpdated(state);
      }
    );


    // Vehículo eliminado
    this.hubConnection.on(
      'VehicleDeleted',
      (
        vehicleId: string
      ) => {
        onVehicleDeleted(vehicleId);
      }
    );


    try {

      await this.hubConnection.start();

      console.log(
        'SignalR connected'
      );

    } catch (error) {

      console.error(
        'SignalR connection failed',
        error
      );
    }
  }


  async stop(): Promise<void> {

    if (!this.hubConnection) {
      return;
    }

    await this.hubConnection.stop();

    this.hubConnection =
      undefined;
  }
}