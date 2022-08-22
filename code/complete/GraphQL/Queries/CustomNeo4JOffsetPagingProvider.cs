using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data.Neo4J.Execution;
using HotChocolate.Internal;
using HotChocolate.Resolvers;
using HotChocolate.Types.Pagination;
using Neo4j.Driver;

namespace ConferencePlanner.GraphQL.Queries;

/// <summary>
/// An offset paging provider for Neo4J that create pagination queries
/// </summary>
public class CustomNeo4JOffsetPagingProvider : QueryableOffsetPagingProvider
{
    private static readonly MethodInfo _createHandler =
        typeof(CustomNeo4JOffsetPagingProvider).GetMethod(
            nameof(CreateHandlerInternal),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    public override bool CanHandle(IExtendedType source)
    {
        return typeof(INeo4JExecutable).IsAssignableFrom(source.Source) ||
               source.Source.IsGenericType &&
               source.Source.GetGenericTypeDefinition() is { } type &&
               type == typeof(Neo4JExecutable<>);
    }

    protected override OffsetPagingHandler CreateHandler(
        IExtendedType source,
        PagingOptions options)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return (OffsetPagingHandler)_createHandler
            .MakeGenericMethod(source.ElementType?.Source ?? source.Source)
            .Invoke(
                null,
                new object[]
                {
                    options
                })!;
    }

    private static Neo4JOffsetPagingHandler<TEntity> CreateHandlerInternal<TEntity>(
        PagingOptions options) => new(options);

    private sealed class Neo4JOffsetPagingHandler<TEntity> : OffsetPagingHandler
    {
        public Neo4JOffsetPagingHandler(PagingOptions options) : base(options)
        {
        }

        protected override ValueTask<CollectionSegment> SliceAsync(
            IResolverContext context,
            object source,
            OffsetPagingArguments arguments)
        {
            var f = CreatePagingContainer(source);
            return ResolveAsync(context, f, arguments);
        }

        private Neo4JExecutable<TEntity> CreatePagingContainer(object source)
        {
            return source switch
            {
                Neo4JExecutable<TEntity> nfe => nfe,
                _ => throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage(
                            "The provided source is not supported for Neo4j paging",
                            source.GetType().FullName ?? source.GetType().Name)
                        .SetCode(ErrorCodes.Data.NoPaginationProviderFound)
                        .Build())
            };
        }

        private async ValueTask<CollectionSegment> ResolveAsync(
            IResolverContext context,
            Neo4JExecutable<TEntity> queryable,
            OffsetPagingArguments arguments = default)
        {
            // if (arguments.Take.HasValue)
            // {
            //     queryable = queryable.WithLimit(arguments.Take.Value + 1);
            // }
            //
            // if (arguments.Skip.HasValue)
            // {
            //     queryable = queryable.WithSkip(arguments.Skip.Value);
            // }

            var hasPreviousPage = arguments.Skip > 0;

            var statement = queryable.Pipeline();
            if (arguments.Skip.HasValue)
            {
                statement.Skip(arguments.Skip.Value);
            }
            if (arguments.Take.HasValue)
            {
                statement.Limit(arguments.Take.Value + 1);
            }

            var cursor = statement.Build();
            
            var items = new List<TEntity>();
            var session = queryable.Source as IAsyncSession;
            var results = await session.RunAsync(cursor).ConfigureAwait(false);
            while (await results.FetchAsync().ConfigureAwait(false))
            {
                var value = ValueMapper.MapValue<TEntity>(results.Current[0]);
                items.Add(value);
            }
            
            var hasNextPage = items.Count > arguments.Take;

            if (hasNextPage)
            {
                items.RemoveAt(items.Count - 1);
            }

            var segmentInfo = new CollectionSegmentInfo(hasNextPage, hasPreviousPage);
            return new CollectionSegment((IReadOnlyCollection<object>) items, segmentInfo);
        }
    }
}