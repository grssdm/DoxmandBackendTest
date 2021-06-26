using System.Collections.Generic;
using DoxmandAPI.DTOs;
using DoxmandAPI.Models;
using DoxmandBackend.DTOs;

namespace DoxmandAPI.Repos
{
    public interface IUsersRepo
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(string id);
        User AddUserToFirebase(UserDTO userDto);
        User AddProductToUser(User user, Product product);
        void DeleteUser(User user);
    }
}