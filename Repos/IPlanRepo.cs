using DoxmandAPI.Models;
using DoxmandBackend.DTOs;
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
        Plan AddPlanToFirebase(PlanDTO planDto);
        Plan EditPlan(Plan plan);
        void DeletePlanById(string planId);
        string FindPlanNameByProduct(string productId);
    }
}
