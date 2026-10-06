import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel
} from '@microsoft/signalr';

import { VehicleStateUpdated } from '../models/vehicle.model';

@Injectable({
  providedIn: 'root'
})
export class RealtimeService {

  private hubConnection?: HubConnection;

  async start(
    onVehicleUpdated:
      (state: VehicleStateUpdated) => void
  ): Promise<void> {

    this.hubConnection =
      new HubConnectionBuilder()
        .withUrl(
          'https://localhost:7051/hubs/fleet'
        )
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    this.hubConnection.on(
      'VehicleStateUpdated',
      (state: VehicleStateUpdated) => {
        onVehicleUpdated(state);
      });

    try {
      await this.hubConnection.start();

      console.log(
        'SignalR connected'
      );
    }
    catch (error) {
      console.error(
        'SignalR connection error',
        error
      );
    }
  }

  async stop(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
    }
  }
}
