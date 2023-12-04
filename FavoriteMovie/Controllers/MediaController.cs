using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FavoriteMovie.Data;
using FavoriteMovie.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FavoriteMovie.ViewsModels.Medias;

namespace FavoriteMovie.Controllers
{
    [Authorize]
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MediaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*----------------------------------------------------------------
         * Data
         * --------------------------------------------------------------- */
        private async Task<(List<Models.MediaType> allMediaTypes, List<Models.MediaGenre> allMediaGenres)> GetMediaTypesAndGenresAsync()
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
              return _context.Media != null ? 
                          View(await _context.Media.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Media'  is null.");
        }

        // Partials Medias with Download Url
        public async Task<IActionResult> GetMediasWithStreamingLink(int pageIndex = 1, int pageSize = DefaultPageSize, string sortField = "MediaType", string sortOrder = "asc")
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

            // 3) Créer une requête LINQ pour récupérer les médias avec URL de téléchargement en base de données
            var mediasWithUrlQuery = _context.Media
                .Include(m => m.MediaType)
                .Include(m => m.MediasGenres)
                .Where(m => m.StreamingLink != null)
                .Select(m => new MediaViewModel
                {
                    Media = m,
                    IsFavorite = favoriteMediaIds.Contains(m.Id),  // Vérifier si le média est parmi les favoris de l'utilisateur   
                }); ;

            // 4) Footer List
            // Récupérer le nombre total de médias dans la base de données
            var totalMediasCount = await _context.Media.CountAsync();

            // Récupérer le nombre total de médias avec URL de téléchargement dans la base de données
            var totalMediasStreamingLinkCount = await _context.Media.CountAsync(m => m.StreamingLink != null);

            // 5) Pagination
            // Créer une liste paginée de médias avec URL de téléchargement
            var paginatedMediasWithUrl = await PaginatedList<MediaViewModel>.CreateAsync(mediasWithUrlQuery, pageIndex, pageSize);

            // 6) Créer le modèle de vue pour la vue partielle
            var viewModel = new MediaViewModel
            {
                MediasStreamingLink = paginatedMediasWithUrl,  // Liste paginée des médias avec URL de téléchargement
                TotalMediasCount = totalMediasCount,             // Nombre total de médias dans la base de données
                TotalMediasWithStreamingLinkCount = totalMediasStreamingLinkCount  // Nombre total de médias avec URL de téléchargement dans la base de données
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

            // 3) Créer une requête pour récupérer les médias en attente (sans URL de téléchargement)
            var mediasWaitingQuery = _context.Media
                              .Include(m => m.MediaType)
                              .Include(m => m.MediasGenres)
                              .Where(m => m.StreamingLink == null)
                              .Select(m => new MediaViewModel
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
            var paginatedMediasWaiting = await PaginatedList<MediaViewModel>.CreateAsync(mediasWaitingQuery, pageIndex, pageSize);

            // 6) Créer le modèle de vue pour la vue partielle
            var viewModel = new MediaViewModel
            {
                MediasWaiting = paginatedMediasWaiting,  // Liste paginée des médias en attente
                TotalMediasCount = totalMediasCount,     // Nombre total de médias dans la base de données
                TotalMediasWaitingCount = totalMediasWaitingCount  // Nombre total de médias en attente dans la base de données
            };

            // 7) Renvoyer la vue partielle avec le modèle de vue
            return PartialView("Partials/_MediasWaiting", viewModel);
        }

        // GET: Media/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .FirstOrDefaultAsync(m => m.Id == id);
            if (media == null)
            {
                return NotFound();
            }

            return View(media);
        }

        // GET: Media/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Media/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Note,CreatedAt,UpdatedAt,AllocineDescription,AllocineLink,StreamingLink")] Media media)
        {
            if (ModelState.IsValid)
            {
                _context.Add(media);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(media);
        }

        // GET: Media/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media.FindAsync(id);
            if (media == null)
            {
                return NotFound();
            }
            return View(media);
        }

        // POST: Media/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Note,CreatedAt,UpdatedAt,AllocineDescription,AllocineLink,StreamingLink")] Media media)
        {
            if (id != media.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(media);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MediaExists(media.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(media);
        }

        // GET: Media/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Media == null)
            {
                return NotFound();
            }

            var media = await _context.Media
                .FirstOrDefaultAsync(m => m.Id == id);
            if (media == null)
            {
                return NotFound();
            }

            return View(media);
        }

        // POST: Media/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Media == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Media'  is null.");
            }
            var media = await _context.Media.FindAsync(id);
            if (media != null)
            {
                _context.Media.Remove(media);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MediaExists(int id)
        {
          return (_context.Media?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
