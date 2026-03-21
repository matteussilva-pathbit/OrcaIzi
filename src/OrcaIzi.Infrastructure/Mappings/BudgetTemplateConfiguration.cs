﻿namespace OrcaIzi.Infrastructure.Mappings
{
    public class BudgetTemplateConfiguration : IEntityTypeConfiguration<BudgetTemplate>
    {
        public void Configure(EntityTypeBuilder<BudgetTemplate> builder)
        {
            builder.ToTable("BudgetTemplates");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.Observations).HasMaxLength(1000).IsRequired(false);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.BudgetTemplate)
                .HasForeignKey(x => x.BudgetTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}



