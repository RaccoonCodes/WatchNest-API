using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WatchNest.DTO;
using WatchNest.Models.Interfaces;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Caching.Distributed;
using WatchNest.Extensions;

namespace WatchNest.Models.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApiUsers> _userManager;
        private readonly IDistributedCache _distributedCache;

        public AdminService(ApplicationDbContext context, UserManager<ApiUsers> userManager, IDistributedCache distributedCache)
            => (_context, _userManager, _distributedCache) = (context, userManager, distributedCache);

        //T(x) = O(n) where n is related series to user
        //T(x) = O(1) where there is little to no series with the user
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        //T(x) = O(n + pageSize) where n is counting total matching records

        public async Task<RestDTO<IEnumerable<object>>> GetAllUsersAsync(string baseurl, string rel, string action, int pageIndex, int pageSize)
        {
            if (pageIndex < 0 || pageSize <= 0)
            {
                return new RestDTO<IEnumerable<object>>()
                {
                    Data = Enumerable.Empty<object>(),
                    Message = "Invalid pagination parameters. Ensure 'pageIndex' >= 0 and 'pageSize' > 0."
                };
            }
            var usersQuery = _context.Users.Select(u => new
            {
                u.Id,
                u.UserName
            });

            var totalUsers = await usersQuery.CountAsync();

            //Caching here 
            var cacheKey = $"Users-{pageIndex}-{pageSize}";

            IEnumerable<object> users = Enumerable.Empty<object>();

            if (!_distributedCache.TryGetValue(cacheKey, out users!))
            {
                users = await usersQuery
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

                _distributedCache.Set(cacheKey, users, new TimeSpan(0, 3, 0));
            }

            if (!users.Any())
            {
                return new RestDTO<IEnumerable<object>>()
                {
                    Data = Enumerable.Empty<object>(),
                    Message = $"There are no users with page index: {pageIndex} and pagesize: {pageSize}" +
                    $"\nTotal user record: {totalUsers}"
                };
            };

            var totalpages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var links = PaginationHelper.GeneratePaginationLinks(baseurl, rel, action, pageIndex, pageSize, totalpages);

            return new RestDTO<IEnumerable<object>>
            {
                Data = users,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = totalUsers,
                TotalPages = totalpages,
                Links = links,
                Message = "Sucessfully retrieved all paginated users"
            };
        }

        //T(x) = O(n log n + pageSize) without filter
        //T(x) = O(n * m + n log n + pageSize) with filter
        public async Task<RestDTO<IEnumerable<string>>> GetAllSeriesAsync(AdminRequestDTO<SeriesDTO> input, string baseUrl, string rel, string action)
        {
            var query = _context.Series.AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
            {
                query = query.Where(q => q.TitleWatched.Contains(input.FilterQuery));
            }
            if (!string.IsNullOrEmpty(input.SortColumn) && !string.IsNullOrEmpty(input.SortOrder))
            {
                query = query.OrderBy($"{input.SortColumn} {input.SortOrder}");
            }

            //Cache 
            var cacheKey = input.GenerateCacheKey();
            IEnumerable<string> uniqueTitles = Enumerable.Empty<string>();

            if(!_distributedCache.TryGetValue(cacheKey, out uniqueTitles!))
            {
                uniqueTitles = await query.Select(s => s.TitleWatched).Distinct().ToListAsync();
                _distributedCache.Set(cacheKey, uniqueTitles, new TimeSpan(0, 3, 0));

            }
            
            var paginatedTitles = uniqueTitles.Skip(input.PageIndex * input.PageSize).Take(input.PageSize);

            var totalRecords = uniqueTitles.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / input.PageSize);

            var links = PaginationHelper.GeneratePaginationLinks(
                baseUrl, 
                rel, 
                action, 
                input.PageIndex, 
                input.PageSize, 
                totalPages,
                new Dictionary<string, string> {
                    { "SortColumn", input.SortColumn ?? string.Empty },
                    { "SortOrder", input.SortOrder ?? string.Empty },
                    { "FilterQuery", input.FilterQuery ?? string.Empty }
                }
            );

            return new RestDTO<IEnumerable<string>>
            {
                Data = paginatedTitles,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = totalRecords,
                TotalPages = totalPages,
                Message = "Successfully retrieved paginated unqiue series with or without filter",
                Links =links
            };
        }

    }
}
