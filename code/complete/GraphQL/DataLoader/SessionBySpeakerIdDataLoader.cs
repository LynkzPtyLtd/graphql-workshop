using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.GraphQL.Data;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.GraphQL.DataLoader;

public class SessionBySpeakerIdDataLoader : GroupedDataLoader<int, Session>
{
    private static readonly string SessionCacheKey = GetCacheKeyType<SessionByIdDataLoader>();
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public SessionBySpeakerIdDataLoader(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory ??
                            throw new ArgumentNullException(nameof(dbContextFactory));
    }

    protected override async Task<ILookup<int, Session>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        await using var dbContext = 
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var list = await dbContext.Speakers
            .Where(s => keys.Contains(s.Id))
            .Include(s => s.SessionSpeakers)
            .SelectMany(s => s.SessionSpeakers)
            .Include(s => s.Session)
            .ToListAsync(cancellationToken);

        TryAddToCache(SessionCacheKey, list, item => item.SessionId, item => item.Session!);

        return list.ToLookup(t => t.SpeakerId, t => t.Session!);
    }
}