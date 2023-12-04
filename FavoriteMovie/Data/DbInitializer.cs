using FavoriteMovie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FavoriteMovie.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Appliquer les migrations
                dbContext.Database.Migrate();

                // Créer les rôles et l'utilisateur administrateur
                await CreateRolesAndAdminUserAsync(scope.ServiceProvider);

                // Initialiser les mediaType et les mediasGenres
                await InitializeMediaTypeAndMediaGenreAsync(dbContext, scope.ServiceProvider);

            }
        }

        private static async Task InitializeMediaTypeAndMediaGenreAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            // Vérifier si les mediaType existent déjà
            if (!dbContext.MediaType.Any())
            {
                // Créer des mediaType par défaut
                var mediaTypeData = new List<string>
                    {
                        "Film",
                        "Série",
                        "Anime",
                        "Dessin animé"
                    };

                foreach (var type in mediaTypeData)
                {
                    dbContext.MediaType.Add(new MediaType { Name = type });
                }

                await dbContext.SaveChangesAsync();
            }

            // Vérifier si les mediasGenres existent déjà
            if (!dbContext.MediaGenre.Any())
            {
                // Créer des mediasGenres par défaut
                var mediasGenresData = new List<string>
                    {
                        "Action",
                        "Animation",
                        "Aventure",
                        "Biopic",
                        "Comédie",
                        "Comédie Dramatique",
                        "Drame",
                        "Épouvante Horreur",
                        "Famille",
                        "Fantastique",
                        "Guerre",
                        "Historique",
                        "Musical",
                        "Policier",
                        "Romance",
                        "Science Fiction",
                        "Thriller",
                        "Western",
                        "Sexe",
                        "Drogue",
                        "Documentaire"
                     };

                foreach (var genre in mediasGenresData)
                {
                    dbContext.MediaGenre.Add(new MediaGenre { Name = genre });
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private static async Task CreateRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create roles
            var roles = new[] { "Admin", "Member" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create admin user
            string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            string adminUserName = Environment.GetEnvironmentVariable("ADMIN_USERNAME");

            // Check if admin user exists
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                // Create a new instance of IdentityUser for the admin
                var admin = new IdentityUser
                {
                    UserName = adminUserName,
                    Email = adminEmail
                };

                // Create the admin user in the database
                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    // If creation is successful, assign the 'Admin' role to the user
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
