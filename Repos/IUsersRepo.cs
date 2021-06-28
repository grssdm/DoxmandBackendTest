using System.Collections.Generic;
using DoxmandBackend.DTOs;
using DoxmandBackend.Models;

namespace DoxmandBackend.Repos
{
    public interface IUsersRepo
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(string id);
        User AddUserToFirebase(UserDTO userDto);
        User EditUser(User user);
        void DeleteUser(User user);
    }
}