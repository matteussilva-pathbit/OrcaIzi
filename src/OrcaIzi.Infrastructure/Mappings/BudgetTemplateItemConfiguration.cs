﻿namespace OrcaIzi.Infrastructure.Mappings
{
    public class BudgetTemplateItemConfiguration : IEntityTypeConfiguration<BudgetTemplateItem>
    {
        public void Configure(EntityTypeBuilder<BudgetTemplateItem> builder)
        {
            builder.ToTable("BudgetTemplateItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        }
    }
}



