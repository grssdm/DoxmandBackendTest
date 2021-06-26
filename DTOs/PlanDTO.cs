using DoxmandBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoxmandBackend.DTOs
{
    public class PlanDTO
    {
        public Dictionary<string, Coord> PlacedProducts { get; set; }
        public string Name { get; set; }
    }
}
