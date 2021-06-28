using DoxmandBackend.DTOs;
using DoxmandBackend.Models;
using System.Collections.Generic;

namespace DoxmandBackend.Repos
{
    interface IPlanRepo
    {
        IEnumerable<Plan> GetAllPlans();
        Plan GetPlanById(string planId);
        Plan AddPlanToFirebase(PlanDTO planDto);
        bool AddPlanToUser(User user, Plan plan);
        Plan EditPlan(Plan plan);
        void DeletePlanById(string planId);
        string FindPlanNameByProduct(string productId);
    }
}
