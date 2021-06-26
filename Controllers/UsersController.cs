using System;
using System.Collections.Generic;
using DoxmandAPI.DTOs;
using DoxmandAPI.Repos;
using DoxmandAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoxmandBackend.Models;
using DoxmandBackend.DTOs;

namespace DoxmandAPI.Controllers
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
        public ActionResult<User> AddProductToUser(string id, string productId)
        {
            // Ha a gyártási szám üres vagy null, akkor 400-as hiba
            if (string.IsNullOrEmpty(productId))
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
                    return NotFound($"There is no user with ID {id}");
                }

                // Termék megkeresése
                var products = _repo.GetAllProducts();

                // Ha nincsenek termékek az adatbázisban, akkor 404-es hiba
                if (products == null)
                {
                    return NotFound("There are no products");
                }

                // A megtalált termékeken végigiterálunk
                foreach (var product in products)
                {
                    // Megvizsgáljuk a gyártási számot
                    if (product.Product_ID.Equals(productId))
                    {
                        // Végig iterálunk a felhasználó termékein
                        foreach (var userProduct in user.Products)
                        {
                            // Ha megtaláljuk közöttük a megadott gyártási számmal
                            // rendelkező terméket, akkor 400-as hiba
                            if (userProduct.Product_ID.Equals(productId))
                            {
                                return BadRequest($"User with ID {id} already contains product with ID {productId}");
                            }
                        }

                        // Ellenkező esetben hozzáadjuk az adott felhasználóhoz
                        _repo.AddProductToUser(user, product);
                        // És 200-as státuszkóddal térünk vissza
                        return Ok($"Product with ID {productId} has been successfully added to User with ID {id}");
                    }
                }

                // Ha nem találtuk meg a terméket a megadott gyártási számmal, akkor 404-es hiba
                return NotFound($"There is no product with ID {productId}");
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
        public ActionResult Authenticate([FromBody] UserCred userCred)
        {
            // Ha valamelyik attribútum üres vagy null, akkor 400-as hiba
            if (string.IsNullOrEmpty(userCred.Email) || string.IsNullOrEmpty(userCred.Password))
            {
                return BadRequest("Some parameters are missing");
            }

            var token = _jwtAuthenticationManager.Authenticate(userCred.Email, userCred.Password);
            
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

            foreach (KeyValuePair<string, Coord> placedProduct in planDto.PlacedProducts)
            {
                var product = _repo.GetProductById(placedProduct.Key);

                if (product == null)
                {
                    return NotFound($"There is no Product with ID {placedProduct.Key}");
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
    }
}