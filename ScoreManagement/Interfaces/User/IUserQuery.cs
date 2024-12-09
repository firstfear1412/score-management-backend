using ScoreManagement.Model.Table;
using ScoreManagement.Model;

namespace ScoreManagement.Interfaces
{
    public interface IUserQuery
    {
        Task<User?> GetUser(UserResource resource);
        Task<bool> UpdateUser(User resource, string query);
    }
}
