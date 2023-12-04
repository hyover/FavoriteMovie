using FavoriteMovie.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FavoriteMovie.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Media> Media { get; set; }
        public DbSet<MediaFavorite> MediaFavorite { get; set; }
        public DbSet<MediaGenre> MediaGenre { get; set; } = default!;
        public DbSet<MediaType> MediaType { get; set; } = default!;
    }
}