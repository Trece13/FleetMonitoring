using Fleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fleet.Infrastructure.SqlServer.Configurations;

public sealed class ProcessedMessageConfiguration
    : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(
        EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");

        builder.HasKey(x => x.EventId);
    }
}