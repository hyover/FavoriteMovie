﻿using FavoriteMovie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FavoriteMovie.ViewsModels.Media
{
    public class CreateMediaViewModel
    {
        // Required
        [Required(ErrorMessage = "S'il te plait entre un nom pour le média.")]
        [StringLength(255, ErrorMessage = "Le nom doit avoir une longueur comprise entre {2} and {1}.", MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "S'il te plait entre courte note.")]
        [StringLength(255, ErrorMessage = "La note doit avoir une longueur comprise entre {2} and {1}.", MinimumLength = 5)]
        public string Note { get; set; }

        // Optional
        public string? AllocineDescription { get; set; }
        public string? AllocineLink { get; set; }
        public string? StreamingLink { get; set; }

        [Display(Name = "Selected Media Genre")]
        public string SelectedMediaGenre { get; set; }

        [Display(Name = "Selected Media Type")]
        public int SelectedMediaType { get; set; }

    }
}
