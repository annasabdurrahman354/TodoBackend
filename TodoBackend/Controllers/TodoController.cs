using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoBackend.Models;
using TodoBackend.Repositories;

namespace TodoBackend.Controllers
{
    [Route("api/todo")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepository todoRepository;

        public TodoController(ITodoRepository todoRepository)
        {
            this.todoRepository = todoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoModel>>> GetAll()
        {
            try
            {
                return (await todoRepository.GetAll()).ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TodoModel>>> GetUserTodos(string userId)
        {
            try
            {
                return (await todoRepository.GetUserTodos(userId)).ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoModel>> Get(int id)
        {
            try
            {
                var result = await todoRepository.Get(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TodoModel>> Add([FromBody] TodoModel todo)
        {
            try
            {
                if (todo == null)
                    return BadRequest();

                var newTodo = await todoRepository.Add(todo);

                return CreatedAtAction(nameof(Get),
                    new { id = newTodo.Id }, newTodo);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new todo record");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TodoModel>> Update(int id, [FromBody] TodoModel todo)
        {
            try
            {
                if (id != todo.Id)
                    return BadRequest("Todo's Id mismatch");

                var updatedTodo = await todoRepository.Get(id);

                if (updatedTodo == null)
                    return NotFound($"Todo with Id = {id} not found");

                return await todoRepository.Update(todo);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TodoModel>> Delete(int id)
        {
            try
            {
                var todoToDelete = await todoRepository.Get(id);

                if (todoToDelete == null)
                {
                    return NotFound($"Todo with Id = {id} not found");
                }

                return await todoRepository.Delete(id);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
