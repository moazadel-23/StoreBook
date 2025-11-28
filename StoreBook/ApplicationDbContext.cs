using Microsoft.EntityFrameworkCore;
using StoreBook.Models;

namespace StoreBook
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Auther> Authers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<AutherBook> AutherBooks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Key
            modelBuilder.Entity<AutherBook>()
                .HasKey(c => new { c.AutherId, c.BookId });

        }
    }

}
