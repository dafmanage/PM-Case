using Microsoft.EntityFrameworkCore;

namespace PM_Case_Managemnt_Implementation.Helpers
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }

        public PagedList(List<T> items, MetaData metaData)
        {
            MetaData = metaData;
            AddRange(items);
        }

        public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            var metaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            return new PagedList<T>(items, metaData);
        }

    }

    public class PagedDataResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public object MetaData { get; set; } = new { };

        public PagedDataResponse(List<T> data, object metaData)
        {
            Data = data;
            MetaData = metaData;
        }
    }
}
