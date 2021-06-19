using System.Collections.Generic;

namespace DoxmandAPI.Models
{
    public class User
    {
        public User(string email, string password, string name)
        {
            Email = email;
            Password = password;
            Name = name;
            Products = new List<Product>();
            Plans = new List<Plan>();
        }
        
        public string User_ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; }
        public List<Plan> Plans { get; set; }
    }
}