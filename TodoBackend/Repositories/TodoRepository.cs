using System.Collections.Generic;
using System.Threading.Tasks;
using TodoBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TodoBackend.Authentication;

namespace TodoBackend.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext appDbContext;

        public TodoRepository(ApplicationDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<IEnumerable<TodoModel>> GetAll()
        {
            return await appDbContext.Todo.ToListAsync();
        }

        public async Task<TodoModel> Get(int id)
        {
            return await appDbContext.Todo.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<TodoModel> Add(TodoModel todo)
        {
            var result = await appDbContext.Todo.AddAsync(todo);
            await appDbContext.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<TodoModel> Update(TodoModel todo)
        {
            var result = await appDbContext.Todo.FirstOrDefaultAsync(e => e.Id == todo.Id);

            if (result != null)
            {
                result.Title = todo.Title;
                result.Description = todo.Description;
                result.ImageName = todo.ImageName;
                result.Status = todo.Status;

                await appDbContext.SaveChangesAsync();

                return result;
            }

            return null;
        }

        public async Task<TodoModel> Delete(int id)
        {
            var result = await appDbContext.Todo
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result != null)
            {
                appDbContext.Todo.Remove(result);
                await appDbContext.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<TodoModel>> GetUserTodos(string id)
        {
            return await appDbContext.Todo.Where(e => e.UserId == id).ToListAsync(); ;
        }
    }
}
