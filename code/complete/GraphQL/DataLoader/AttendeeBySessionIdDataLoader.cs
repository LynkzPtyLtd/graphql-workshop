using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.GraphQL.Data;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.GraphQL.DataLoader;

public class AttendeeBySessionIdDataLoader : GroupedDataLoader<int, Attendee>
{
    private static readonly string AttendeeCacheKey = GetCacheKeyType<AttendeeByIdDataLoader>();
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public AttendeeBySessionIdDataLoader(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory ??
                            throw new ArgumentNullException(nameof(dbContextFactory));
    }

    protected override async Task<ILookup<int, Attendee>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var list = await dbContext.Sessions
            .Where(s => keys.Contains(s.Id))
            .Include(s => s.SessionAttendees)
            .SelectMany(s => s.SessionAttendees)
            .Include(s => s.Attendee)
            .ToListAsync(cancellationToken);

        TryAddToCache(AttendeeCacheKey, list, item => item.AttendeeId, item => item.Attendee!);

        return list.ToLookup(t => t.SessionId, t => t.Attendee!);
    }
}