using System;
using System.Collections.Generic;
using System.Text;

namespace GameReviews.Mobile.Models
{
    public class Item
    {
        public string Id { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FunFact { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
