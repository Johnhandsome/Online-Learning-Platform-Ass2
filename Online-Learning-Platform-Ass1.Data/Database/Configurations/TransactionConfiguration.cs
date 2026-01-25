using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Database.Configurations;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        // Create unique index on TransactionGateId to prevent duplicate processing
        builder.HasIndex(t => t.TransactionGateId)
            .IsUnique()
            .HasFilter("[transaction_gate_id] IS NOT NULL");

        builder.Property(t => t.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.TransactionGateId)
            .HasMaxLength(100);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18, 2)");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("pending");

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Transactions)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
