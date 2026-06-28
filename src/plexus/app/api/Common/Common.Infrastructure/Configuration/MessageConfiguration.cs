
using Common.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Infrastructure.Configuration;

internal class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder
            .HasKey(m => m.Id);

        builder
            .Property<string>("serializedData")
            .IsRequired()
            .HasField("serializedData");

        builder
            .Property(m => m.Type)
            .IsRequired()
            .HasConversion(
                t => t.AssemblyQualifiedName,
                t => Type.GetType(t!)!);

        builder
            .Ignore(m => m.Data);
    }
}