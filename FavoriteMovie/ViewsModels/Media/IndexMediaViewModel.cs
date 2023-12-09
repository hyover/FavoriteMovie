using FavoriteMovie.Helpers;
using FavoriteMovie.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FavoriteMovie.ViewsModels.Media
{
    public class IndexMediaViewModel
    {
        // Pagination
        public PaginatedList<IndexMediaViewModel> MediasStreamingLink { get; set; }
        public PaginatedList<IndexMediaViewModel> MediasWaiting { get; set; }

        // Media entity
        public Models.Media Media { get; set; }

        // Media Favorite
        public bool IsFavorite { get; set; }

        // Total de tous les films
        public int TotalMediasCount { get; set; }

        // Total de films avec DownloadUrl
        public int TotalMediasWithStreamingLinkCount { get; set; }

        // Total de films en attente
        public int TotalMediasWaitingCount { get; set; }

    }
}
