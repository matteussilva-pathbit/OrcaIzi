﻿namespace OrcaIzi.Infrastructure.Mappings
{
    public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("CatalogItems");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Unit).HasMaxLength(50);
            builder.Property(x => x.Category).HasMaxLength(100);
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.UserId, x.Name });
        }
    }
}




