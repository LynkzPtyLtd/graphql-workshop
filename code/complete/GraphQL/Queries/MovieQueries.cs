using System.Collections.Generic;
using System.Linq;
using ConferencePlanner.GraphQL.Models;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Data.Neo4J.Execution;
using HotChocolate.Types;
using Neo4j.Driver;

namespace ConferencePlanner.GraphQL.Queries;

[ExtendObjectType(Name = "Query")]
public class MovieQueries
{
    [GraphQLName("actors")]
    [UseNeo4JDatabase(databaseName: "neo4j")]
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IExecutable<Person> GetActors(
        [ScopedService] IAsyncSession session) =>
        new Neo4JExecutable<Person>(session);

    [GraphQLName("actors")]
    [UseNeo4JDatabase(databaseName: "neo4j")]
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetActors() =>
        new List<Person>
        {
            new()
            {

            }
        }.AsQueryable();

    [GraphQLName("movies")]
    [UseNeo4JDatabase(databaseName: "neo4j")]
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IExecutable<Movie> GetMovies(
        [ScopedService] IAsyncSession session) =>
        new Neo4JExecutable<Movie>(session);
}