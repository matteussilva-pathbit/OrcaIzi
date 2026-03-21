﻿namespace OrcaIzi.Infrastructure.Context
{
    public class OrcaIziDbContext : IdentityDbContext<User>
    {
        public OrcaIziDbContext(DbContextOptions<OrcaIziDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetItem> BudgetItems { get; set; }
        public DbSet<BudgetTemplate> BudgetTemplates { get; set; }
        public DbSet<BudgetTemplateItem> BudgetTemplateItems { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrcaIziDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}



