using BlazorDataService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorDataService.Services
{
    public interface IToDoListService
    {
        Task<List<ToDo>> Get();
        Task<PaginatedList<ToDo>> GetList(int? pageNumber, string sortField, string sortOrder);
        Task<ToDo> Get(int id);
        Task<ToDo> Add(ToDo toDo);
        Task<ToDo> Update(ToDo toDo);
        Task<ToDo> Delete(int id);
    }
    public class ToDoListService : IToDoListService
    {
        private readonly ApplicationDbContext _context;

        public ToDoListService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<ToDo>> Get()
        {
            return await _context.ToDoList.ToListAsync();
        }

        public async Task<PaginatedList<ToDo>> GetList(int? pageNumber, string sortField, string sortOrder)
        {
            int pageSize = 5;
            var toDoList = _context.ToDoList.OrderByDynamic(sortField, sortOrder.ToUpper());
            return await PaginatedList<ToDo>.CreateAsync(toDoList.AsNoTracking(), pageNumber ?? 1, pageSize);
        }

        public async Task<ToDo> Get(int id)
        {
            var toDo = await _context.ToDoList.FindAsync(id);
            return toDo;
        }

        public async Task<ToDo> Add(ToDo toDo)
        {
            _context.ToDoList.Add(toDo);
            await _context.SaveChangesAsync();
            return toDo;
        }

        public async Task<ToDo> Update(ToDo toDo)
        {
            _context.Entry(toDo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return toDo;
        }

        public async Task<ToDo> Delete(int id)
        {
            var toDo = await _context.ToDoList.FindAsync(id);
            _context.ToDoList.Remove(toDo);
            await _context.SaveChangesAsync();
            return toDo;
        }

        
    }

   public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderByMember, string direction)
        {
            var queryElementTypeParam = Expression.Parameter(typeof(T));
            var memberAccess = Expression.PropertyOrField(queryElementTypeParam, orderByMember);
            var keySelector = Expression.Lambda(memberAccess, queryElementTypeParam);

            var orderBy = Expression.Call(
                typeof(Queryable),
                direction == "ASC" ? "OrderBy" : "OrderByDescending",
                new Type[] { typeof(T), memberAccess.Type },
                query.Expression,
                Expression.Quote(keySelector));

            return query.Provider.CreateQuery<T>(orderBy);
        }
    }
}


