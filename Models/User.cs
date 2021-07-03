using System.Collections.Generic;

namespace DoxmandBackend.Models
{
    public class User
    {
        public User(string userId)
        {
            User_ID = userId;
            Products = new List<Product>();
            Plans = new List<Plan>();
        }

        /*
        public User(string email, string password, string username)
        {
            Email = email;
            Password = password;
            Username = username;
            Products = new List<Product>();
            Plans = new List<Plan>();
        }
        */
        
        public string User_ID { get; set; }
        /*
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        */
        public List<Product> Products { get; set; }
        public List<Plan> Plans { get; set; }
    }
}