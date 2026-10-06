using Fleet.Contracts.Telemetry;
using FluentValidation;

namespace Fleet.Application.Telemetry;

public sealed class ReceiveTelemetryValidator
    : AbstractValidator<ReceiveTelemetryRequest>
{
    public ReceiveTelemetryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.Timestamp)
            .NotEmpty();
    }
}