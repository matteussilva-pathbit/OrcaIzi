using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaIzi.Domain.Entities;

namespace OrcaIzi.Infrastructure.Mappings
{
    public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
    {
        public void Configure(EntityTypeBuilder<BudgetItem> builder)
        {
            builder.ToTable("BudgetItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        }
    }
}
