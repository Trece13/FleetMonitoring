using Fleet.Domain.Enums;
using Fleet.Domain.Models;
using Fleet.Domain.Services;

namespace Fleet.Domain.Entities;

public sealed class VehicleState
{
    private const double StationaryThresholdMeters = 5;
    private static readonly TimeSpan StopThreshold =
        TimeSpan.FromMinutes(1);

    public Guid VehicleId { get; private set; }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public DateTime LastSeenAtUtc { get; private set; }

    public DateTime? StationarySinceUtc { get; private set; }

    public VehicleStatus Status { get; private set; }

    public bool StopAlertRaised { get; private set; }

    private VehicleState()
    {
    }

    public VehicleState(
        Guid vehicleId,
        double latitude,
        double longitude,
        DateTime recordedAtUtc)
    {
        VehicleId = vehicleId;

        Latitude = latitude;
        Longitude = longitude;

        LastSeenAtUtc = recordedAtUtc;

        Status = VehicleStatus.Unknown;
    }

    public PositionUpdateResult ApplyPosition(
        double latitude,
        double longitude,
        DateTime recordedAtUtc)
    {
        //
        // Protección contra eventos fuera de orden.
        //
        if (recordedAtUtc <= LastSeenAtUtc)
        {
            return new PositionUpdateResult(
                IsStale: true,
                StopAlertShouldBeCreated: false);
        }

        var distanceMeters =
            GeoDistanceCalculator.CalculateMeters(
                Latitude,
                Longitude,
                latitude,
                longitude);

        var alertShouldBeCreated = false;

        if (distanceMeters <= StationaryThresholdMeters)
        {
            HandleStationaryPosition(
                recordedAtUtc,
                ref alertShouldBeCreated);
        }
        else
        {
            HandleMovement();
        }

        Latitude = latitude;
        Longitude = longitude;
        LastSeenAtUtc = recordedAtUtc;

        return new PositionUpdateResult(
            IsStale: false,
            StopAlertShouldBeCreated:
                alertShouldBeCreated);
    }

    private void HandleStationaryPosition(
        DateTime recordedAtUtc,
        ref bool alertShouldBeCreated)
    {
        StationarySinceUtc ??= LastSeenAtUtc;

        var stationaryDuration =
            recordedAtUtc - StationarySinceUtc.Value;

        if (stationaryDuration < StopThreshold)
        {
            Status = VehicleStatus.Stationary;
            return;
        }

        Status = VehicleStatus.Stopped;

        if (StopAlertRaised)
            return;

        StopAlertRaised = true;
        alertShouldBeCreated = true;
    }

    private void HandleMovement()
    {
        Status = VehicleStatus.Moving;

        StationarySinceUtc = null;

        StopAlertRaised = false;
    }
}