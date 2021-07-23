using DoxmandBackend.DTOs;
using System.Collections.Generic;

namespace DoxmandBackend.Models
{
    public class Plan
    {
        public Plan()
        {
            PlacedProducts = null;
            Name = "";
            Room_ID = -1;
        }

        public Plan(List<PlanProduct> placedProducts, string name, long roomId)
        {
            PlacedProducts = placedProducts;
            Name = name;
            Room_ID = roomId;
        }

        public Plan(PlanDTO planDto)
        {
            PlacedProducts = planDto.PlacedProducts;
            Room_ID = planDto.Room_ID;
            Name = planDto.Name;
        }

        public string Plan_ID { get; set; }
        public List<PlanProduct> PlacedProducts { get; set; }
        public string Name { get; set; }
        public class PlanProduct
        {
            public Product Product { get; set; }
            public Coord Location { get; set; }
            public int Rotation { get; set; }
        }
        public class Coord
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
        public long Room_ID { get; set; }
    }
}