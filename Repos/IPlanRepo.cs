using DoxmandAPI.Models;
using DoxmandBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoxmandBackend.Repos
{
    interface IPlanRepo
    {
        IEnumerable<Plan> GetAllPlans();
        Plan GetPlanById(string planId);
        User AddPlanToUser(User user, Plan plan);
        Plan AddPlanToFirebase(Dictionary<string, Coord> placedProducts);
        Plan EditPlan(Plan plan);
        void DeletePlanById(string planId);
    }
}
