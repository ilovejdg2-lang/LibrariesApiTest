using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Services
{
    public class BooksService : IBooksService
    {
        private readonly LibraryContext _context;

        public BooksService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> Get(int libraryId, int[] ids)
        {
            var query = _context.Books
                .Where(x => x.LibraryId == libraryId)
                .AsQueryable();

            if (ids != null && ids.Any())
                query = query.Where(x => ids.Contains(x.Id));

            return await query.ToListAsync();
        }

        public async Task<Book> Add(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book> Update(Book book)
        {
            var existingBook = await _context.Books.SingleAsync(x => x.Id == book.Id);
            existingBook.Name = book.Name;
            existingBook.Category = book.Category;
            existingBook.LibraryId = book.LibraryId;
            await _context.SaveChangesAsync();
            return existingBook;
        }

        public async Task<bool> Delete(Book book)
        {
            _context.Books.Remove(book);
            return await _context.SaveChangesAsync() > 0;
        }
    }

    public interface IBooksService
    {
        Task<IEnumerable<Book>> Get(int libraryId, int[] ids);

        Task<Book> Add(Book book);

        Task<Book> Update(Book book);

        Task<bool> Delete(Book book);
    }
}
