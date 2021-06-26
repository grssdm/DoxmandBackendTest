using DoxmandBackend.Models;
using System.Collections.Generic;

namespace DoxmandAPI.Models
{
    public class Plan
    {
        public Plan(Dictionary<string, Coord> placedProducts, string name)
        {
            PlacedProducts = placedProducts;
            Name = name;
        }

        public string Plan_ID { get; set; }
        public Dictionary<string, Coord> PlacedProducts { get; set; }
        public string Name { get; set; }
    }
}