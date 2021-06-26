using DoxmandAPI.Models;
using DoxmandAPI.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoxmandBackend.Controllers
{
    [Authorize]
    [Route("api/plans")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly DoxmandRepo _repo = new DoxmandRepo();

        [HttpGet]
        public ActionResult<IEnumerable<Plan>> GetAllPlans()
        {
            var plans = _repo.GetAllPlans();

            if (plans == null)
            {
                return NotFound($"There are no Plans");
            }

            return Ok(plans);
        }

        [HttpGet("{id}")]
        public ActionResult<Plan> GetPlanById(string id)
        {
            var plan = _repo.GetPlanById(id);

            if (plan == null)
            {
                return NotFound($"There is no Plan with ID {id}");
            }

            return Ok(plan);
        }

        [HttpPut("{id}")]
        public ActionResult<Plan> EditPlan(Plan plan)
        {
            var _plan = _repo.GetPlanById(plan.Plan_ID);

            if (_plan == null)
            {
                return NotFound($"There is no Plan with ID {plan.Plan_ID}");
            }

            try
            {
                _repo.EditPlan(plan);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeletePlan(string id)
        {
            var plan = _repo.GetPlanById(id);

            if (plan == null)
            {
                return NotFound($"There is no Plan with ID {id}");
            }


            // Try-catch a FireBase miatt
            try
            {
                _repo.DeletePlanById(id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return NoContent();
        }

        [HttpGet("find/{id}")]
        public ActionResult<string> FindPlanNameByProduct(string id)
        {
            var product = _repo.GetProductById(id);

            if (product == null)
            {
                return NotFound($"There is no Product with ID {id}");
            }

            var name = _repo.FindPlanNameByProduct(id);

            if (name == null)
            {
                return NotFound($"There is no Plan that contains the Product with ID {id}");
            }

            return Ok(name);
        }
    }
}
