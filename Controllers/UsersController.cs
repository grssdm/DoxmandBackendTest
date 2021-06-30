using System;
using System.Collections.Generic;
using DoxmandBackend.DTOs;
using DoxmandBackend.Repos;
using DoxmandBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoxmandBackend.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // A Repo-ban vannak az adatbázishoz köthető függvények
        private readonly DoxmandRepo _repo = new DoxmandRepo();
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;

        public UsersController(IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }
        
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
        public ActionResult<User> GetUserById(string id)
        {
            // Felhasználó megkeresése a megadott ID alapján
            var user = _repo.GetUserById(id);

            // Ha null, akkor nincs ilyen felhasználó, 404-es hiba
            if (user == null)
            {
                return NotFound($"There is no user with ID {id}");
            }

            // Visszatérünk a megtalált felhasználóval
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult<User> AddNewUser(UserDTO userDto)
        {
            // Ha valamelyik attribútum üres vagy null, akkor 400-as hiba
            if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password) || string.IsNullOrEmpty(userDto.Username))
            {
                return BadRequest("Some parameters are missing");
            }

            // A meglévő felhasználók vizsgálata
            var users = _repo.GetAllUsers();
            foreach (var user in users)
            {
                // Ha létezik már ilyen email címmel rendelkező felhasználó, akkor 400-as hiba
                if (user.Email.Equals(userDto.Email))
                {
                    return BadRequest($"User with Email {userDto.Email} already registered");
                }
            }
            
            // Try-catch a FireBase miatt
            try
            {
                // Létrehozzuk a felhasználót a megadott adatokkal
                _repo.AddUserToFirebase(userDto);
                // és visszatérünk a tokennel, valamint 200-as státuszkóddal
                var token = _jwtAuthenticationManager.Authenticate(userDto.Email, userDto.Password);
                return Ok(token);
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}/products")]
        public ActionResult<IEnumerable<Product>> GetProductsByUser(string id)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                return NotFound($"There is no User with ID {id}");
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
                bool conflict = false;

                foreach (var productDto in productDtos)
                {
                    var _product = _repo.CheckUserProductsForName(user, productDto.SavedName);

                    if (_product != null)
                    {
                        return BadRequest("PRODUCT_WITH_NAME_ALREADY_EXISTS");
                    }

                    Product product = _repo.AddProductToFirebase(productDto);
                    var isConflict = _repo.AddProductToUser(user, product);
                    if (isConflict)
                    {
                        conflict = true;
                    }
                }

                return conflict ? Ok("CONFLICT") : Ok("PRODUCTS_ADDED");
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<User> DeleteUser(string id)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                return NotFound($"There is no User with ID {id}");
            }
            
            // Try-catch a FireBase miatt
            try
            {
                // Felhasználó törlése
                _repo.DeleteUser(user);
            }
            // Ha valami hiba történt a FireBase területén, akkor 500-as hiba
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
            // 204-es státuszkóddal való visszatérés
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody] LoginDTO loginDto)
        {
            // Ha valamelyik attribútum üres vagy null, akkor 400-as hiba
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Some parameters are missing");
            }

            var token = _jwtAuthenticationManager.Authenticate(loginDto.Email, loginDto.Password);
            
            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        [HttpGet("{id}/plans")]
        public ActionResult<IEnumerable<Plan>> GetPlansByUser(string id)
        {
            // Felhasználó megkeresése
            var user = _repo.GetUserById(id);

            // Ha nincs felhasználó a megadott ID-val, akkor 404-es hiba
            if (user == null)
            {
                return NotFound($"There is no User with ID {id}");
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