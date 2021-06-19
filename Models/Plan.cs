using DoxmandBackend.Models;
using System.Collections.Generic;

namespace DoxmandAPI.Models
{
    public class Plan
    {
        public Plan(Dictionary<string, Coord> placedProducts)
        {
           PlacedProducts = placedProducts;
        }

        public string Plan_ID { get; set; }
        public Dictionary<string, Coord> PlacedProducts { get; set; }
    }
}