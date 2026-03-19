using System;
using System.Collections.Generic;
using System.Text;

namespace GameReviews.Mobile.Models
{
    public class Game
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Developer { get; set; } = "";
        public string Platform { get; set; } = "";
        public string ReleaseYear { get; set; } = "";
    }
}
