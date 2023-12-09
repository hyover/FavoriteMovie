using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FavoriteMovie.ViewsModels.Media
{
    public class EditMediaViewModel
    {
        public Models.Media Media { get; set; }

        [Display(Name = "Selected Media Genre")]
        public string SelectedMediaGenre { get; set; }

        [Display(Name = "Selected Media Type")]
        public string SelectedMediaType { get; set; }

        public ICollection<SelectListItem> AllMediasTypes { get; set; }
        public ICollection<SelectListItem> AllMediasGenres { get; set; }
    }
}
