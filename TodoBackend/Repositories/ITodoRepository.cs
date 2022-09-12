using System.Collections.Generic;
using System.Threading.Tasks;
using TodoBackend.Models;

namespace TodoBackend.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoModel>> GetAll();
        Task<IEnumerable<TodoModel>> GetUserTodos(string userId);
        Task<TodoModel> Get(int id);
        Task<TodoModel> Add(TodoModel todo);
        Task<TodoModel> Update(TodoModel todo);
        Task<TodoModel> Delete(int id);
    }
}
