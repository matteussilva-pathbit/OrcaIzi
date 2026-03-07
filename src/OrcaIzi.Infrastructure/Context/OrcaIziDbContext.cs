using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrcaIzi.Domain.Entities;

namespace OrcaIzi.Infrastructure.Context
{
    public class OrcaIziDbContext : IdentityDbContext<User>
    {
        public OrcaIziDbContext(DbContextOptions<OrcaIziDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetItem> BudgetItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrcaIziDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
