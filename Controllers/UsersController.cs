using System;
using System.Collections.Generic;
using DoxmandBackend.DTOs;
using DoxmandBackend.Repos;
using DoxmandBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;

namespace DoxmandBackend.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // A Repo-ban vannak az adatbázishoz köthető függvények
        private readonly DoxmandRepo _repo = new DoxmandRepo();
        private readonly List<int> _badRequestErrorCodes = new List<int>() { 0, 1, 2, 6, 7, 8, 10 };
        private readonly List<int> _unauthorizedErrorCodes = new List<int>() { 3, 4 };
        private readonly List<int> _internalServerErrorCodes = new List<int>() { 9, 11, 12, 13, 14, 15 };
        
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            // Felhasználók lekérése
            var users = _repo.GetAllUsers();

            // Ha null, akkor nincs még felhasználó, 404-es hiba
            if (users == null)
            {
                return NotFound("There are no users");
            }

            // Visszatérünk a felhasználókkal, 200-as státuszkóddal
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserById(string id)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                try
                {
                    var authUser = await FirebaseAuth.DefaultInstance.GetUserAsync(id);

                    User newUser = new User(id);

                    return Ok(_repo.EditUser(newUser));
                } catch (Exception ex)
                {
                    return Problem(ex.Message);
                }
            }

            return Ok(user);
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByUser(string id)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                try
                {
                    var authUser = await FirebaseAuth.DefaultInstance.GetUserAsync(id);

                    User newUser = new User(id);

                    _repo.EditUser(newUser);

                    return (newUser.Products);
                }
                catch (Exception ex)
                {
                    return Problem(ex.Message);
                }
            }

            return Ok(user.Products);
        }

        [HttpPut("{id}/products/add")]
        public ActionResult<User> AddProductToUser(string id, [FromBody] ProductDTO productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Some parameters are missing");
            }

            // Try-catch a FireBase miatt
            try
            {
                // Felhasználó megkeresése
                var user = _repo.GetUserById(id);

                // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
                if (user == null)
                {
                    return NotFound("NOT_FOUND_USER");
                }

                var _product = _repo.CheckUserProductsForName(user, productDto.SavedName);

                if (_product != null)
                {
                    return BadRequest("PRODUCT_WITH_NAME_ALREADY_EXISTS");
                }

                Product product = _repo.AddProductToFirebase(productDto);

                var conflict = _repo.AddProductToUser(user, product);

                return conflict ? Ok("CONFLICT") : Ok("PRODUCT_ADDED");
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}/products/add/more")]
        public ActionResult<User> AddProductsToUser(string id, [FromBody] List<ProductDTO> productDtos)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            if (productDtos.Count == 0)
            {
                return BadRequest("EMPTY_PRODUCT_ARRAY");
            }

            // Try-catch a FireBase miatt
            try
            {
                var newProducts = new List<Product>();

                foreach (var productDto in productDtos)
                {
                    var _product = _repo.CheckUserProductsForName(user, productDto.SavedName);

                    if (_product != null)
                    {
                        return BadRequest("PRODUCT_WITH_NAME_ALREADY_EXISTS");
                    }

                    Product product = _repo.AddProductToFirebase(productDto);
                    _repo.AddProductToUser(user, product);
                    newProducts.Add(product);
                }

                return Ok(newProducts);
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var authUser = await FirebaseAuth.DefaultInstance.GetUserAsync(id);

                var user = _repo.GetUserById(id);

                if (user != null)
                {
                    _repo.DeleteUser(user);
                }

                _ = FirebaseAuth.DefaultInstance.DeleteUserAsync(id);

                return NoContent();
            }
            catch (FirebaseAuthException ex)
            {
                if (_internalServerErrorCodes.Contains((int) ex.ErrorCode))
                {
                    return StatusCode(500, ex.Message);
                } else if (_badRequestErrorCodes.Contains((int)ex.ErrorCode))
                {
                    return StatusCode(400, ex.Message);
                } else if (_unauthorizedErrorCodes.Contains((int)ex.ErrorCode))
                {
                    return StatusCode(401, ex.Message);
                }

                return StatusCode(404, ex.Message);
            }
        }

        [HttpGet("{id}/plans")]
        public async Task<ActionResult<IEnumerable<Plan>>> GetPlansByUser(string id)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                try
                {
                    var authUser = await FirebaseAuth.DefaultInstance.GetUserAsync(id);

                    User newUser = new User(id);

                    _repo.EditUser(newUser);

                    return (newUser.Plans);
                }
                catch (Exception ex)
                {
                    return Problem(ex.Message);
                }
            }

            return Ok(user.Plans);
        }

        [HttpPut("{id}/plans/add")]
        public ActionResult AddPlanToUser(string id, [FromBody] PlanDTO planDto)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound($"There is no User with ID {id}");
            }

            if (planDto.PlacedProducts.Count == 0)
            {
                return BadRequest("You have not placed any products");
            }

            var _plan = _repo.CheckUserPlansForName(user, planDto.Name);

            if (_plan != null)
            {
                return BadRequest("PLAN_WITH_NAME_ALREADY_EXISTS");
            }

            foreach (var placedProduct in planDto.PlacedProducts)
            {
                if (placedProduct.Product.Product_ID != "")
                {
                    var product = _repo.GetProductById(placedProduct.Product.Product_ID);

                    if (product == null)
                    {
                        return NotFound($"There is no Product with ID {placedProduct.Product.Product_ID}");
                    }
                }
            }

            // Try-catch a FireBase miatt
            try
            {
                var plan = _repo.AddPlanToFirebase(planDto);

                _repo.AddPlanToUser(user, plan);

                return Ok($"Plan with ID {plan.Plan_ID} has been successfully added to User with ID {user.User_ID}");
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}/plans/add/more")]
        public ActionResult AddPlansToUser(string id, [FromBody] List<PlanDTO> planDtos)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            if (planDtos.Count == 0)
            {
                return BadRequest("EMPTY_PLAN_ARRAY");
            }

            foreach (var planDto in planDtos)
            {
                if (planDto.PlacedProducts.Count == 0)
                {
                    return BadRequest("NO_PLACED_PRODUCT");
                }

                var _plan = _repo.CheckUserPlansForName(user, planDto.Name);

                if (_plan != null)
                {
                    return BadRequest("PLAN_WITH_NAME_ALREADY_EXISTS");
                }

                foreach (var placedProduct in planDto.PlacedProducts)
                {
                    if (placedProduct.Product.Product_ID != "")
                    {
                        var product = _repo.GetProductById(placedProduct.Product.Product_ID);

                        if (product == null)
                        {
                            return NotFound("NOT_FOUND_PRODUCT");
                        }
                    }
                }
            }

            // Try-catch a FireBase miatt
            try
            {
                bool conflict = false;
                foreach (var planDto in planDtos)
                {
                    var plan = _repo.AddPlanToFirebase(planDto);
                    var isConflict = _repo.AddPlanToUser(user, plan);
                    if (isConflict)
                    {
                        conflict = true;
                    }
                }

                return conflict ? Ok("CONFLICT") : Ok("PLANS_ADDED");
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}/plans")]
        public ActionResult EditUserPlans(string id, List<Plan> plans)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            if (plans.Count == 0)
            {
                // Try-catch a FireBase miatt
                try
                {
                    user.Plans = plans;
                    _repo.EditUser(user);

                    return Ok("PLANS_EDITED");
                }
                // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            List<Plan> newList = new List<Plan>();
            List<PlanDTO> planDtos = new List<PlanDTO>();

            foreach (var plan in plans)
            {
                if (!string.IsNullOrEmpty(plan.Plan_ID))
                {
                    newList.Add(plan);
                } else
                {
                    planDtos.Add(new PlanDTO
                    {
                        PlacedProducts = plan.PlacedProducts,
                        Name = plan.Name,
                        Room_ID = plan.Room_ID
                    });
                }
            }

            // Try-catch a FireBase miatt
            try
            {
                user.Plans = newList;
                var newUser = _repo.EditUser(user);
                bool conflict = false;
                foreach (var planDto in planDtos)
                {
                    var plan = _repo.AddPlanToFirebase(planDto);
                    var isConflict = _repo.AddPlanToUser(user, plan);
                    if (isConflict)
                    {
                        conflict = true;
                    }
                }

                return conflict ? Ok("CONFLICT") : Ok("PLANS_EDITED");
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}/products/{productId}")]
        public ActionResult DeleteUserProduct(string id, string productId)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            var product = _repo.GetProductById(productId);

            if (product == null)
            {
                return NotFound("NOT_FOUND_PRODUCT");
            }

            // Try-catch a FireBase miatt
            try
            {

                _repo.DeleteProductFromUser(user, productId);
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            // 204-es státuszkóddal való visszatérés
            return NoContent();
        }

        [HttpDelete("{id}/plans/{planId}")]
        public ActionResult DeleteUserPlan(string id, string planId)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            var plan = _repo.GetPlanById(planId);

            if (plan == null)
            {
                return NotFound("NOT_FOUND_PLAN");
            }

            // Try-catch a FireBase miatt
            try
            {

                _repo.DeletePlanFromUser(user, planId);
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            // 204-es státuszkóddal való visszatérés
            return NoContent();
        }

        [HttpDelete("{id}/products/more")]
        public ActionResult DeleteUserProducts(string id, List<string> productIds)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            // Try-catch a FireBase miatt
            try
            {
                foreach (var productId in productIds)
                {
                    var product = _repo.GetProductById(productId);

                    if (product == null)
                    {
                        return NotFound("NOT_FOUND_PRODUCT");
                    }

                    _repo.DeleteProductFromUser(user, productId);
                }
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            // 204-es státuszkóddal való visszatérés
            return NoContent();
        }

        [HttpDelete("{id}/plans/more")]
        public ActionResult DeleteUserPlans(string id, List<string> planIds)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                return NotFound("NOT_FOUND_USER");
            }

            // Try-catch a FireBase miatt
            try
            {
                foreach (var planId in planIds)
                {
                    var plan = _repo.GetPlanById(planId);

                    if (plan == null)
                    {
                        return NotFound("NOT_FOUND_PRODUCT");
                    }

                    _repo.DeletePlanFromUser(user, planId);
                }
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            // 204-es státuszkóddal való visszatérés
            return NoContent();
        }
    }
}