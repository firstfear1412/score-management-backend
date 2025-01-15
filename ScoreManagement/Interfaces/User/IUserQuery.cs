using ScoreManagement.Model;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Interfaces
{
    public interface IUserQuery
    {
        Task<User?> GetUser(UserResource resource);
        Task<User?> GetUserInfo(UserResource resource);
        Task<bool> updateUserByConditionQuery(UserResource resource);
        Task<bool> UpdateUser(User resource, string query);

        Task<List<UserResource>> GetAllUsers();

        Task<bool> InsertUser(UserResource resource);

        Task<bool> CheckEmailExist(string email);

        Task<bool> UpdateUserById(UserResource resource, string query);
    }
}