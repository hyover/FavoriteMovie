using FavoriteMovie.Models;

namespace FavoriteMovie.ViewsModels.Medias
{
    public class MediaViewModel
    {
        // Pagination
        public PaginatedList<MediaViewModel>? MediasWithDownloadUrl { get; set; }
        public PaginatedList<MediaViewModel>? MediasWaiting { get; set; }

        // Model Media
        public Media Media { get; set; }

        // Media Favorite
        public bool IsFavorite { get; set; }

        // Total de tous les films
        public int TotalMediasCount { get; set; }

        // Total de films avec DownloadUrl
        public int TotalMediasWithDownloadUrlCount { get; set; }

        // Total de films en attente
        public int TotalMediasWaitingCount { get; set; } 

        // Medias Genre selectionnés dans la liste déroulante
        public string SelectedMediaGenre { get; set; }
    }
}
