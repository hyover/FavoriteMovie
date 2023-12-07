using FavoriteMovie.Helpers;
using FavoriteMovie.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FavoriteMovie.ViewsModels.Media
{
    public class MediaViewModel
    {
        // Pagination
        public PaginatedList<MediaViewModel>? MediasStreamingLink { get; set; }
        public PaginatedList<MediaViewModel>? MediasWaiting { get; set; }

        // Media Favorite
        public bool IsFavorite { get; set; }

        // Total de tous les films
        public int TotalMediasCount { get; set; }

        // Total de films avec DownloadUrl
        public int TotalMediasWithStreamingLinkCount { get; set; }

        // Total de films en attente
        public int TotalMediasWaitingCount { get; set; }
        
        // ID Media Type selectionné dans la liste déroulante
        public string SelectedMediaType { get; set; }
        // ID Medias Genre selectionnés dans la liste déroulante
        public string SelectedMediaGenre { get; set; }

    }
}
