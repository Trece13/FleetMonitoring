export interface Vehicle {
  id: string;
  vehicleId: string;
  name: string;
  latitude: number | null;
  longitude: number | null;
  status: string;
  lastSeenAtUtc: string | null;
}

export interface VehicleStateUpdated {
  vehicleId: string;
  latitude: number;
  longitude: number;
  status: string;
  lastSeenAtUtc: string;
}

export interface TelemetryHistoryItem {
  eventId: string;
  latitude: number;
  longitude: number;
  recordedAtUtc: string;
}