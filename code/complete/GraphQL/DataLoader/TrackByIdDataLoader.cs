using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ConferencePlanner.GraphQL.Data;
using GreenDonut;

namespace ConferencePlanner.GraphQL.DataLoader;

public class TrackByIdDataLoader : BatchDataLoader<int, Track>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public TrackByIdDataLoader(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory ?? 
                            throw new ArgumentNullException(nameof(dbContextFactory));
    }

    protected override async Task<IReadOnlyDictionary<int, Track>> LoadBatchAsync(
        IReadOnlyList<int> keys, 
        CancellationToken cancellationToken)
    {
        await using var dbContext = 
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Tracks
            .Where(s => keys.Contains(s.Id))
            .ToDictionaryAsync(t => t.Id, cancellationToken);
    }
}