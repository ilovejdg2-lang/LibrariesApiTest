using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.WebAPI.Services
{
    public class LibrariesService : ILibrariesService
    {
        private readonly LibraryContext _context;

        public LibrariesService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Library>> Get(int[] ids)
        {
            var query = _context.Libraries.AsQueryable();

            if (ids != null && ids.Any())
                query = query.Where(x => ids.Contains(x.Id));

            return await query.ToListAsync();
        }

        public async Task<Library> Add(Library library)
        {
            _context.Libraries.Add(library);
            await _context.SaveChangesAsync();
            return library;
        }

        public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects)
        {
            var list = projects.ToList();
            _context.Libraries.AddRange(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<Library> Update(Library library)
        {
            var projectForChanges = await _context.Libraries.SingleAsync(x => x.Id == library.Id);
            projectForChanges.Name = library.Name;
            projectForChanges.Location = library.Location;
            await _context.SaveChangesAsync();
            return projectForChanges;
        }

        public async Task<bool> Delete(Library library)
        {
            _context.Libraries.Remove(library);
            return await _context.SaveChangesAsync() > 0;
        }
    }

    public interface ILibrariesService
    {
        Task<IEnumerable<Library>> Get(int[] ids);

        Task<Library> Add(Library library);

        Task<Library> Update(Library library);

        Task<bool> Delete(Library library);
    }
}
