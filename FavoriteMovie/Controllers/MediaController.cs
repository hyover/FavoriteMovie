using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FavoriteMovie.Data;
using FavoriteMovie.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FavoriteMovie.ViewsModels.Media;
using Microsoft.AspNetCore.Identity;
using FavoriteMovie.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FavoriteMovie.Controllers
{
    [Authorize]
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MediaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*----------------------------------------------------------------
         * Data
         * --------------------------------------------------------------- */
        private async Task<(ICollection<Models.MediaType> allMediaTypes, ICollection<Models.MediaGenre> allMediaGenres)> GetMediaTypesAndGenresAsync()
        {
            var allMediaTypes = await _context.MediaType.ToListAsync();
            var allMediaGenres = await _context.MediaGenre.ToListAsync();

            return (allMediaTypes, allMediaGenres);
        }

        /*----------------------------------------------------------------
         * Constants
         * --------------------------------------------------------------- */
        private const int DefaultPageSize = 8;


        /* ---------------------------------------------------------------- 
         * View Index of Medias
         * ---------------------------------------------------------------- */

        // GET: Media
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // Partials Medias with Download Url
        public async Task<IActionResult> GetMediasStreamingLink(int pageIndex = 1, int pageSize = DefaultPageSize, string sortField = "MediaType", string sortOrder = "asc")
        {
            // 1) Récupérer l'identifiant de l'utilisateur actuel
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2) Récupérer les identifiants des médias favoris de l'utilisateur et les stocker dans un HashSet
            var favoriteMediaIds = new HashSet<int>(
                await _context.MediaFavorite
                              .Where(f => f.User.Id == userId)
                              .Select(f => f.Media.Id)
                              .ToListAsync()
            );

            // 3) Créer une requête LINQ pour récupérer les médias avec URL de streaming en base de données
            var mediasWithUrlQuery = _context.Media
                .Include(m => m.MediaType)
                .Include(m => m.MediasGenres)
                .Where(m => m.StreamingLink != null)
                .Select(m => new IndexMediaViewModel
                {
                    Media = m,
                    IsFavorite = favoriteMediaIds.Contains(m.Id),  // Vérifier si le média est parmi les favoris de l'utilisateur   
                }); ;

            // 4) Footer List
            // Récupérer le nombre total de médias dans la base de données
            var totalMediasCount = await _context.Media.CountAsync();

            // Récupérer le nombre total de médias avec URL de streaming dans la base de données
            var totalMediasStreamingLinkCount = await _context.Media.CountAsync(m => m.StreamingLink != null);

            // 5) Pagination
            // Créer une liste paginée de médias avec URL de streaming
            var paginatedMediasWithUrl = await PaginatedList<IndexMediaViewModel>.CreateAsync(mediasWithUrlQuery, pageIndex, pageSize);

            // 6) Créer le modèle de vue pour la vue partielle
            var viewModel = new IndexMediaViewModel
            {
                MediasStreamingLink = paginatedMediasWithUrl,  // Liste paginée des médias avec URL de streaming
                TotalMediasCount = totalMediasCount,             // Nombre total de médias dans la base de données
                TotalMediasWithStreamingLinkCount = totalMediasStreamingLinkCount  // Nombre total de médias avec URL de streaming dans la base de données
            };

            // 7) Renvoyer la vue partielle avec le modèle de vue
            return PartialView("Partials/_MediasStreamingLink", viewModel);
        }

        // Partials Medias without Download Url
        public async Task<IActionResult> GetMediasWaiting(int pageIndex = 1, int pageSize = DefaultPageSize)
        {
            // 1) Récupérer l'identifiant de l'utilisateur actuel
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2) Récupérer les identifiants des médias favoris de l'utilisateur et les stocker dans une HashSet
            var favoriteMediaIds = new HashSet<int>(
                await _context.MediaFavorite
                              .Where(f => f.User.Id == userId)
                              .Select(f => f.Media.Id)
                              .ToListAsync()
            );

            // 3) Créer une requête pour récupérer les médias en attente (sans URL de streaming)
            var mediasWaitingQuery = _context.Media
                              .Include(m => m.MediaType)
                              .Include(m => m.MediasGenres)
                              .Where(m => m.StreamingLink == null)
                              .Select(m => new IndexMediaViewModel
                              {
                                  Media = m,
                                  IsFavorite = favoriteMediaIds.Contains(m.Id),  // Vérifier si le média est parmi les favoris de l'utilisateur
                              });

            // 4) Footer List
            // Récupérer le nombre total de médias dans la base de données
            var totalMediasCount = await _context.Media.CountAsync();

            // Récupérer le nombre total de médias en attente dans la base de données
            var totalMediasWaitingCount = await _context.Media.CountAsync(m => m.StreamingLink == null);

            // 5) Pagination
            // Créer une liste paginée de médias en attente
            var paginatedMediasWaiting = await PaginatedList<IndexMediaViewModel>.CreateAsync(mediasWaitingQuery, pageIndex, pageSize);

            // 6) Créer le modèle de vue pour la vue partielle
            var viewModel = new IndexMediaViewModel
            {
                MediasWaiting = paginatedMediasWaiting,  // Liste paginée des médias en attente
                TotalMediasCount = totalMediasCount,     // Nombre total de médias dans la base de données
                TotalMediasWaitingCount = totalMediasWaitingCount  // Nombre total de médias en attente dans la base de données
            };

            // 7) Renvoyer la vue partielle avec le modèle de vue
            return PartialView("Partials/_MediasWaiting", viewModel);
        }

        // Toggle Favorite star in list
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] IndexMediaViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingFavorite = await _context.MediaFavorite
                                    .FirstOrDefaultAsync(f => f.Media.Id == model.Media.Id && f.User.Id == userId);

            if (existingFavorite != null)
            {
                // Le film est déjà dans les favoris, le retirer
                _context.MediaFavorite.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Json(new { IsFavorite = false });
            }
            else
            {
                // Le film n'est pas dans les favoris, l'ajouter
                var newFavorite = new MediaFavorite { Media = model.Media, User = model.Media.User };
                _context.MediaFavorite.Add(newFavorite);
                await _context.SaveChangesAsync();
                return Json(new { IsFavorite = true });
            }
        }

        /* ----------------------------------------------------------------
         * View Details foreach Media
         * ---------------------------------------------------------------- */

        // GET: Medias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // 1) Vérifier si l'identifiant du média est null ou si le contexte des médias est null
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            // 2) Récupérer le média avec ses relations (MediaGenres et MediaType) basé sur l'identifiant
            var media = await _context.Media
                .Include(m => m.MediasGenres) // Inclure la relation avec MediaGenres
                .Include(m => m.MediaType)   // Inclure la relation avec MediaType
                .FirstOrDefaultAsync(m => m.Id == id);

            //  3) Vérifier si le média existe 
            if (media == null)
            {
                return NotFound();
            }

            // 4) Créer le modèle de vue pour le média
            var mediaViewModel = new DetailsMediaViewModel
            {
                Media = media,
            };

            // 5) Renvoyer la vue avec le modèle de vue contenant les détails du média
            return View(mediaViewModel);
        }

            /* ----------------------------------------------------------------
             * View Create an Media
             * ---------------------------------------------------------------- */

            // GET: Medias/Create
            public async Task<IActionResult> Create()
            {
                // 1) Initialiser les données nécessaires depuis DbContext via GetMediaTypesAndGenresAsync
                var (allMediaTypes, allMediaGenres) = await GetMediaTypesAndGenresAsync();

                // 2) Stocker les données dans la ViewBag
                ViewBag.AllMediasTypes = allMediaTypes;
                ViewBag.AllMediasGenres = allMediaGenres;

                // 3) Retourner la vue
                return View();
            }


            // POST: Medias/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(CreateMediaViewModel createMediaViewModel)
            {
                // 1) Initialiser les données nécessaires depuis DbContext via GetMediaTypesAndGenresAsync
                var (allMediaTypes, allMediaGenres) = await GetMediaTypesAndGenresAsync();

                // 2) Vérifier si le modèle est valide
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine("Error: " + error.ErrorMessage);
                    }

                    // Stocker les données dans la ViewBag
                    ViewBag.AllMediasTypes = allMediaTypes;
                    ViewBag.AllMediasGenres = allMediaGenres;

                    return View(createMediaViewModel);    
                }
                else
                {
                //System.Diagnostics.Debug.WriteLine("Le ModelState est valide");

                var media = new Media
                {
                    Name = createMediaViewModel.Name,
                    AllocineDescription = createMediaViewModel.AllocineDescription,
                    Note = createMediaViewModel.Note,
                    AllocineLink = createMediaViewModel.AllocineLink,
                    User = _context.Users.Find(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    StreamingLink = createMediaViewModel.StreamingLink,

                    MediaType = await _context.MediaType.FindAsync(createMediaViewModel.SelectedMediaType),

                    MediasGenres = await _context.MediaGenre
                       .Where(g => createMediaViewModel.SelectedMediaGenre
                                       .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse)
                                       .Contains(g.Id))
                                       .ToListAsync(),
            };
                // Mettre à jour la date de mise à jour
                media.BeforeSaveChanges();


                //System.Diagnostics.Debug.WriteLine("Le titre est instancié");

                // 3) Mettre à jour la base de données
                // Ajout du média à la base de données
                _context.Add(media);


                    // Enregistrement des modifications dans la base de données de manière asynchrone
                    await _context.SaveChangesAsync();

                    // 4) Redirection vers l'action "Index" une fois l'ajout effectué
                    return RedirectToAction(nameof(Index));
                }
            }

        /* ----------------------------------------------------------------
        * View Edit an Media
        * ---------------------------------------------------------------- */

        // GET: Medias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // 1) Initialiser les données nécessaires depuis DbContext via GetMediaTypesAndGenresAsync
            var (allMediaTypes, allMediaGenres) = await GetMediaTypesAndGenresAsync();

            // 2) Vérifier si l'identifiant est null ou si le contexte des médias est null
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            // 3) Récupérer le média avec ses relations (MediaGenres et MediaType) basé sur l'identifiant
            var media = await _context.Media
                  .Include(m => m.MediasGenres)
                  .Include(m => m.MediaType)
                  .FirstOrDefaultAsync(m => m.Id == id);

            // 4) Vérifier si le média existe
            if (media == null)
            {
                return NotFound();
            }

            // 5) Vérifier si l'utilisateur connecté est l'auteur du média ou un admin
            var currentUserName = User.Identity;

            if (media.User != currentUserName && !User.IsInRole("Admin"))
            {
                return Forbid(); // Refuser l'accès si l'utilisateur n'est pas l'auteur ou un admin
            }

            // 6) Créer le modèle de vue pour le média
            var editMediaViewModel = new EditMediaViewModel
            {
                Media = new Media
                {
                    Id = media.Id,
                    Name = media.Name,
                    MediaType = media.MediaType,
                    MediasGenres = media.MediasGenres,
                    Note = media.Note,
                    AllocineDescription = media.AllocineDescription,
                    AllocineLink = media.AllocineLink,
                    StreamingLink = media.StreamingLink,
                    User = media.User,
                    CreatedAt = media.CreatedAt,
                    UpdatedAt = media.UpdatedAt,
                },
                SelectedMediaType = media.MediaType?.Id.ToString(),
                SelectedMediaGenre = string.Join(",", media.MediasGenres.Select(g => g.Id)),
                AllMediasTypes = allMediaTypes
                    .Select(mt => new SelectListItem { Value = mt.Id.ToString(), Text = mt.Name })
                    .ToList(),
                AllMediasGenres = allMediaGenres
                    .Select(mg => new SelectListItem { Value = mg.Id.ToString(), Text = mg.Name })
                    .ToList()
            };

            // Mettre à jour la date de mise à jour
            editMediaViewModel.Media.BeforeSaveChanges();

            // 7) Renvoyer la vue avec le modèle de vue contenant les détails du média
            return View(editMediaViewModel);
        }


        // POST: Medias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditMediaViewModel editMediaViewModel)
        {
            // 1) Initialiser les données nécessaires depuis DbContext via GetMediaTypesAndGenresAsync
            var (allMediaTypes, allMediaGenres) = await GetMediaTypesAndGenresAsync();

            // 2) Vérifier si l'identifiant est différent de l'identifiant du média dans le modèle
            if (id != editMediaViewModel.Media.Id)
            {
                return NotFound();
            }

            // 3) Récupérer le média avec ses relations (MediaGenres et MediaType) basé sur l'identifiant
            var mediaToUpdate = await _context.Media
                .Include(m => m.MediasGenres) // Inclure la relation avec MediaGenres
                .FirstOrDefaultAsync(m => m.Id == id);

            // 4) Vérifier si le média existe
            if (mediaToUpdate == null || (mediaToUpdate.User != User.Identity && !User.IsInRole("Admin")))
            {
                return Forbid(); // Refuser l'accès si l'utilisateur n'est pas l'auteur ou un admin
            }

            // 5) Vérifier si le modèle est valide
            if (!ModelState.IsValid)
            {
                // Gérez les erreurs de validation
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + error.ErrorMessage);
                }

                // Réinitialiser les données dans le modèle de vue
                editMediaViewModel.AllMediasTypes = allMediaTypes
                    .Select(mt => new SelectListItem { Value = mt.Id.ToString(), Text = mt.Name })
                    .ToList();
                editMediaViewModel.AllMediasGenres = allMediaGenres
                    .Select(mg => new SelectListItem { Value = mg.Id.ToString(), Text = mg.Name })
                    .ToList();

                return View(editMediaViewModel);
            }
            else
            {
                // Mettez à jour les propriétés du média directement à partir du modèle de vue
                mediaToUpdate.Name = editMediaViewModel.Media.Name;
                mediaToUpdate.AllocineDescription = editMediaViewModel.Media.AllocineDescription;
                mediaToUpdate.Note = editMediaViewModel.Media.Note;
                mediaToUpdate.AllocineLink = editMediaViewModel.Media.AllocineLink;
                mediaToUpdate.StreamingLink = editMediaViewModel.Media.StreamingLink;

                // Assurez-vous que MediaGenres est initialisé
                mediaToUpdate.MediasGenres ??= new List<MediaGenre>();  

                // MediaType
                mediaToUpdate.MediaType = await _context.MediaType.FindAsync(int.Parse(editMediaViewModel.SelectedMediaType));

                // MediaGenres
                var selectedGenres = editMediaViewModel.SelectedMediaGenre
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var existingGenres = mediaToUpdate.MediasGenres
                    .Select(g => g.Id)
                    .ToList();

                // Supprimer les genres qui ne sont plus sélectionnés
                var genresToRemove = existingGenres.Except(selectedGenres).ToList();
                foreach (var genreId in genresToRemove)
                {
                    var genreToRemove = mediaToUpdate.MediasGenres.FirstOrDefault(g => g.Id == genreId);
                    if (genreToRemove != null)
                    {
                        mediaToUpdate.MediasGenres.Remove(genreToRemove);
                    }
                }

                // Ajouter les nouveaux genres sélectionnés
                var genresToAdd = selectedGenres.Except(existingGenres).ToList();

                foreach (var genreId in genresToAdd)
                {
                    var genreToAdd = await _context.MediaGenre.FindAsync(genreId);

                    if (genreToAdd != null)
                    {
                        mediaToUpdate.MediasGenres.Add(genreToAdd);
                    }
                }

                // Enregistrez les modifications dans la base de données
                try
                {
                    _context.Update(mediaToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MediaExists(editMediaViewModel.Media.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirigez vers l'index après la mise à jour réussie
                return RedirectToAction(nameof(Index));
            }
        }


        /* ----------------------------------------------------------------
         * View Delete an Media
         * ---------------------------------------------------------------- */

        // GET: Medias/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            // 1) Vérifier si l'identifiant est null ou si le contexte des médias est null
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            // 2) Récupérer le média avec ses relations (MediaGenres et MediaType) basé sur l'identifiant
            var media = await _context.Media.FindAsync(id);
            if (media == null)
            {
                return NotFound();
            }

            // 3) Vérifier si l'utilisateur connecté est l'auteur du média ou un admin
            var currentUserName = User.Identity; // Récupérer le nom de l'utilisateur connecté

            if (media.User != currentUserName && !User.IsInRole("Admin"))
            {
                return Forbid(); // Refuser l'accès si l'utilisateur n'est pas l'auteur ou un admin
            }

            // 4) Créer le modèle de vue pour le média
            var deleteMediaViewModel = new DeleteMediaViewModel
            {
                Media = media
            };

            // 5) Renvoyer la vue avec le modèle de vue contenant les détails du média
            return View(deleteMediaViewModel);
        }

        // POST: Medias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 1) Vérifier si l'identifiant est null ou si le contexte des médias est null
            var media = await _context.Media.FindAsync(id);

            if (media == null || (media.User != User.Identity && !User.IsInRole("Admin")))
            {
                return Forbid(); // Refuser l'accès si l'utilisateur n'est pas l'auteur ou un admin
            }

            // 2) Supprimer le média de la base de données
            if (media != null)
            {
                _context.Media.Remove(media);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MediaExists(int id)
        {
          return (_context.Media?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
