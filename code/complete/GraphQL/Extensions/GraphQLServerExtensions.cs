using ConferencePlanner.GraphQL.Attendees;
using ConferencePlanner.GraphQL.DataLoader;
using ConferencePlanner.GraphQL.Imports;
using ConferencePlanner.GraphQL.Queries;
using ConferencePlanner.GraphQL.Sessions;
using ConferencePlanner.GraphQL.Speakers;
using ConferencePlanner.GraphQL.Tracks;
using HotChocolate;
using HotChocolate.Data.Neo4J;
using HotChocolate.Data.Neo4J.Projections;
using HotChocolate.Data.Projections;
using HotChocolate.Data.Projections.Handlers;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace ConferencePlanner.GraphQL;

public static class GraphQLServerExtensions
{
    public static IServiceCollection AddRelationalDatabaseGraphQLServer(this IServiceCollection services)
    {
        // This adds the GraphQL server core service and declares a schema.
        services.AddGraphQLServer("Relational")

            // Next we add the types to our schema.
            .AddQueryType()
            .AddMutationType()
            .AddSubscriptionType()

            .AddTypeExtension<AttendeeQueries>()
            .AddTypeExtension<AttendeeMutations>()
            .AddTypeExtension<AttendeeSubscriptions>()
            .AddTypeExtension<AttendeeNode>()
            .AddDataLoader<AttendeeByIdDataLoader>()

            .AddTypeExtension<SessionQueries>()
            .AddTypeExtension<SessionMutations>()
            .AddTypeExtension<SessionSubscriptions>()
            .AddTypeExtension<SessionNode>()
            .AddDataLoader<SessionByIdDataLoader>()
            .AddDataLoader<SessionBySpeakerIdDataLoader>()

            .AddTypeExtension<SpeakerQueries>()
            .AddTypeExtension<SpeakerMutations>()
            .AddTypeExtension<SpeakerNode>()
            .AddDataLoader<SpeakerByIdDataLoader>()
            .AddDataLoader<SessionBySpeakerIdDataLoader>()

            .AddTypeExtension<TrackQueries>()
            .AddTypeExtension<TrackMutations>()
            .AddTypeExtension<TrackNode>()
            .AddDataLoader<TrackByIdDataLoader>()

            .AddType<UploadType>()

            // In this section we are adding extensions like relay helpers,
            // filtering and sorting.
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddGlobalObjectIdentification()

            // we make sure that the db exists and prefill it with conference data.
            .EnsureRelationalDatabaseIsCreated()
            
            // Since we are using subscriptions, we need to register a pub/sub system.
            // for our demo we are using a in-memory pub/sub system.
            .AddInMemorySubscriptions()

            // Last we add support for persisted queries. 
            // The first line adds the persisted query storage, 
            // the second one the persisted query processing pipeline.
            .AddFileSystemQueryStorage("./persisted_queries")
            .UsePersistedQueryPipeline()
            .InitializeOnStartup();

        return services;
    }
    
    public static IServiceCollection AddGraphDatabaseGraphQLServer(this IServiceCollection services)
    {
        // This adds the GraphQL server core service and declares a schema.
        services
            .AddSingleton(ImportRequestExecutorBuilderExtensions.Neo4JDriver.Value)
            .AddGraphQLServer("Graph")

            // Next we add the types to our schema.
            .AddQueryType()
            
            .AddType<MovieQueries>()
            
            //.AddNeo4JPagingProviders()
            .AddOffsetPagingProvider<CustomNeo4JOffsetPagingProvider>()
            .AddNeo4JFiltering()
            .AddNeo4JSorting()
            .AddNeo4JProjections()

            // In this section we are adding extensions like relay helpers,
            // filtering and sorting.
            //.AddGlobalObjectIdentification()

            .EnsureGraphDatabaseIsCreated()

            // Since we are using subscriptions, we need to register a pub/sub system.
            // for our demo we are using a in-memory pub/sub system.
            .AddInMemorySubscriptions()

            // Last we add support for persisted queries. 
            // The first line adds the persisted query storage, 
            // the second one the persisted query processing pipeline.
            //.AddFileSystemQueryStorage("./persisted_queries")
            //.UsePersistedQueryPipeline()
            .InitializeOnStartup();

        return services;
    }
}

public static class Neo4JConfigurationExtensions
{
    public static IRequestExecutorBuilder AddNeo4JProjections(
        this IRequestExecutorBuilder builder,
        string? name = null) =>
        builder.ConfigureSchema(s => s.AddNeo4JProjections(name));

    public static ISchemaBuilder AddNeo4JProjections(
        this ISchemaBuilder builder,
        string? name = null) =>
        builder.AddProjections(x => x.AddNeo4JDefaults(), name);

    public static IProjectionConventionDescriptor AddNeo4JDefaults(
        this IProjectionConventionDescriptor descriptor) =>
        descriptor.Provider(new Neo4JProjectionProvider(x =>
        {
            x.AddNeo4JDefaults();
            x.RegisterOptimizer<QueryablePagingProjectionOptimizer>();
        }));

}