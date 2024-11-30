using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using WatchNest.DTO;
using WatchNest.Models.Interfaces;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WatchNest.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace WatchNest.Models.Implementation
{
    public class SeriesService : ISeriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public SeriesService(ApplicationDbContext context, IDistributedCache distributedCache) 
            => (_context,_distributedCache) = (context,distributedCache);

        //T(x) = O(1)
        public async Task<RestDTO<SeriesModel>> CreateSeriesAsync(SeriesDTO input)
        {
            var newSeries = new SeriesModel
            {
                UserID = input.UserID,
                TitleWatched = input.TitleWatched,
                SeasonWatched = input.SeasonWatched,
                Provider = input.ProviderWatched,
                Genre = input.Genre
            };
            _context.Series.Add(newSeries);
            await _context.SaveChangesAsync();

            
            return new RestDTO<SeriesModel>
            {
                Data = newSeries,
                Message = "The series has been successfully created",
            };

        }
        //T(x) = O(n lg n) where n is the number of series
        //T(x) = O(1) when cache is a Hit
        public async Task<RestDTO<SeriesModel[]>> GetSeriesAsync(RequestDTO<SeriesDTO> input, string baseurl, string rel, string action)
        {
            if(input.PageIndex < 0 || input.PageSize <= 0)
            {
                return new RestDTO<SeriesModel[]>()
                {
                    Data = Array.Empty<SeriesModel>(),
                };
            }

            var query = _context.Series.AsQueryable().Where(b => b.UserID == input.UserID);

            if (!string.IsNullOrEmpty(input.FilterQuery))
            {
                query = query.Where(b => b.TitleWatched.Contains(input.FilterQuery));

            }

            var recordCount = await query.CountAsync();

            if (recordCount == 0)
            {
                return new RestDTO<SeriesModel[]>
                {
                    Data = Array.Empty<SeriesModel>(),
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    RecordCount = recordCount,
                    Message =$"No series for the user with the UserID: {input.UserID}",
                    Links = new List<LinkDTO>()
                };
            }

            SeriesModel[]? result;
            
            var totalPages = (int)Math.Ceiling(recordCount / (double)input.PageSize);

            var cacheKey = input.GenerateCacheKey();

            //caching
            if (!_distributedCache.TryGetValue<SeriesModel[]>(cacheKey, out result))
            {
                result = await query.OrderBy($"{input.SortColumn} {input.SortOrder}")
                             .Skip(input.PageIndex * input.PageSize)
                             .Take(input.PageSize)
                             .ToArrayAsync();
                _distributedCache.Set(cacheKey,result,new TimeSpan(0,3,0));
            }

            var links = PaginationHelper.GeneratePaginationLinks(baseurl,rel,action,
                input.PageIndex,input.PageSize,totalPages, new Dictionary<string, string> {
                    { "SortColumn", input.SortColumn ?? string.Empty },
                    { "SortOrder", input.SortOrder ?? string.Empty },
                    {"UserID",input.UserID ?? string.Empty },
                    { "FilterQuery", input.FilterQuery ?? string.Empty }
                });

            return new RestDTO<SeriesModel[]>
            {
                Data = result!,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                TotalPages = totalPages,
                Message = "Successful retrieval",
                Links =links
            };

        }
        //O(lg n) where n is the number of entries in the table (due to indexing)
        public async Task<RestDTO<SeriesModel?>?> UpdateSeriesAsync(SeriesDTO model, string baseurl, string rel, string action)
        {
            var series = await _context.Series
                .FirstOrDefaultAsync(b => b.UserID == model.UserID && b.SeriesID == model.Id);

            if (series == null)
            {
                return new RestDTO<SeriesModel?>
                {
                    Data = null,
                    Message = "Series does not exist in the database, please try again!"
                };
            }

            series.TitleWatched = model.TitleWatched ?? series.TitleWatched;
            series.Genre = model.Genre ?? series.Genre;
            series.Provider = model.ProviderWatched ?? series.Provider;
            series.SeasonWatched = model.SeasonWatched != 0 ? model.SeasonWatched : series.SeasonWatched;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                    return new RestDTO<SeriesModel?>
                    {
                        Data = null,
                        Message = "A concurrency error occurred. " +
                        "The series has been removed or updated, please refresh and try again "
                    };
            }

            var links = PaginationHelper.GeneratePaginationLinks(baseurl, rel, action,0,1,1);

            return new RestDTO<SeriesModel?>
            {
                Data = series,
                Message = "The series has been successfully updated.",
                Links = links
            };

        }

        //T(x) = O(lg n) where n is seriesID 
        public async Task<RestDTO<SeriesModel?>> DeleteSeriesAsync(int id)
        {
            var series = await _context.Series.FirstOrDefaultAsync(a => a.SeriesID == id);

            if (series == null)
            {
                return new RestDTO<SeriesModel?>
                {
                    Data = null,
                    Message = "Series does not exist! Please try again"
                };
            }
            _context.Series.Remove(series);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new RestDTO<SeriesModel?>
                {
                    Data = null,
                    Message = "Concurrency error! The series has been updated, please refresh and try again"
                };
            }

            return new RestDTO<SeriesModel?>
            {
                Data = series,
                Message = "The series has been successfully deleted.",
                Links = new List<LinkDTO>()
            };

        }

        //T(x) = O(lg n) where n is seriesID 
        public async Task<SeriesModel?> GetSeriesByIdAsync(int id)
        {
            return await _context.Series.Where(s => s.SeriesID == id)
                .Select(s => new SeriesModel
                {
                    UserID = s.UserID,
                    SeriesID = s.SeriesID,
                    TitleWatched = s.TitleWatched,
                    Genre = s.Genre,
                    Provider = s.Provider,
                    SeasonWatched = s.SeasonWatched,
                    RowVersion = s.RowVersion
                })
                .FirstOrDefaultAsync();
        }
    }
}
