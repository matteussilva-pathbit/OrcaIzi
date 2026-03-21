﻿namespace OrcaIzi.Infrastructure.Mappings
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Observations).HasMaxLength(1000).IsRequired(false);
            builder.Property(x => x.DigitalSignature).HasMaxLength(4000).IsRequired(false);
            builder.Property(x => x.PaymentProvider).HasMaxLength(50).IsRequired(false);
            builder.Property(x => x.PaymentExternalId).HasMaxLength(100).IsRequired(false);
            builder.Property(x => x.PaymentStatus).HasMaxLength(50).IsRequired(false);
            builder.Property(x => x.PaymentLink).HasMaxLength(2000).IsRequired(false);
            builder.Property(x => x.PaymentQrCode).HasMaxLength(4000).IsRequired(false);
            builder.Property(x => x.PaymentQrCodeBase64).HasMaxLength(8000).IsRequired(false);
            builder.Property(x => x.PublicShareId).IsRequired(false);
            builder.Property(x => x.PublicShareEnabled).HasDefaultValue(false);
            builder.Property(x => x.PublicShareCreatedAt).IsRequired(false);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Budget)
                .HasForeignKey(x => x.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}



