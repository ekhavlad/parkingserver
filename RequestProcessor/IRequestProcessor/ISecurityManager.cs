using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.RequestProcessor
{
    public interface ISecurityManager
    {
        User CreateUser(User User);
        IEnumerable<User> GetAllUsers();
        void SaveUser(User User);
        void DeleteUser(User User);

        UserRole CreateUserRole(UserRole Role);
        IEnumerable<UserRole> GetAllUserRoles();
        void SaveUserRole(UserRole Role);
        void DeleteUserRole(UserRole Role);

        IEnumerable<ServerRequest> GetAllRequests();
        IEnumerable<ServerRequest> GetAvailableRequests(User User);
        ServerRequest GetRequestById(int Id);
        ServerRequest GetRequestByName(string Name);

        User Login(string Login, string Password);
        bool IsRequestAvailable(int UserId, string RequestName);
    }
}
