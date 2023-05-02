using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.Models;

namespace Library.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<BookModel> Books {get;set;}
    public DbSet<BorrowingEntryModel> BookBorrowings {get;set;}
    public DbSet<GroupModel> Groups {get;set;}

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookModel>()
            .HasMany(b => b.Borrowings)
            .WithOne(bb => bb.Book)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupModel>()
            .HasMany(m => m.Members)
            .WithOne(g => g.Group)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
