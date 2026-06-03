using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryService.WebAPI.Data;
using LibraryService.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace LibraryService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/libraries/{libraryId}/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ILibrariesService _librariesService;
        private readonly IBooksService _booksService;

        public BooksController(IBooksService booksService, ILibrariesService librariesService)
        {
            _librariesService = librariesService;
            _booksService = booksService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var books = await _booksService.Get(libraryId, Array.Empty<int>());
            return Ok(books);
        }

        [HttpGet("{bookId}")]
        [Authorize]
        public async Task<IActionResult> Get(int libraryId, int bookId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var book = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(int libraryId, Book book)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            book.LibraryId = libraryId;
            await _booksService.Add(book);
            return Ok(book);
        }

        [HttpPut("{bookId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int libraryId, int bookId, Book book)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var existingBook = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (existingBook == null)
                return NotFound();

            book.Id = bookId;
            book.LibraryId = libraryId;
            await _booksService.Update(book);
            return NoContent();
        }

        [HttpDelete("{bookId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var existingBook = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
            if (existingBook == null)
                return NotFound();

            await _booksService.Delete(existingBook);
            return NoContent();
        }
    }
}