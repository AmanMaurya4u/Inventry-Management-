using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public class ImsDbContext : DbContext
    {
        public ImsDbContext(DbContextOptions<ImsDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantStock> ProductVariantStock { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SalesItem> SalesItems { get; set; }
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<StockTransferItem> StockTransferItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVariantStock>()
                .HasIndex(x => new { x.ProductVariantId, x.WarehouseId })
                .IsUnique();

            modelBuilder.Entity<PurchaseItem>()
                .Property(x => x.CostPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .Property(x => x.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesItem>()
                .Property(x => x.SellingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesItem>()
                .Property(x => x.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockTransaction>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }

}
