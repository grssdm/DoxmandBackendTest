using System.Collections.Generic;
using static DoxmandBackend.Models.Plan;

namespace DoxmandBackend.DTOs
{
    public class PlanDTO
    {
        public List<PlanProduct> PlacedProducts { get; set; }
        public string Name { get; set; }
        public long Room_ID { get; set; }
    }
}
