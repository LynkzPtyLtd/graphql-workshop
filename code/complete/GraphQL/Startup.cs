using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConferencePlanner.GraphQL.Data;
using HotChocolate;

namespace ConferencePlanner.GraphQL;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddCors(o =>
                o.AddDefaultPolicy(b =>
                    b.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()))

            // First we add the DBContext which we will be using to interact with our
            // Database.
            .AddPooledDbContextFactory<ApplicationDbContext>(
                (s, o) => o
                    .UseSqlite("Data Source=conferences.db")
                    .UseLoggerFactory(s.GetRequiredService<ILoggerFactory>()))

            .AddRelationalDatabaseGraphQLServer();
        //.AddGraphDatabaseGraphQLServer();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors();

        app.UseWebSockets();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", context =>
            {
                context.Response.Redirect("/graphql", false);
                return Task.CompletedTask;
            });

            // We will be using the new routing API to host our GraphQL middleware.
            endpoints.MapGraphQL("/graphql", new NameString("Relational"));
            
            // Neo4j demo for later.
            //endpoints.MapGraphQL("/graphql-graph", new NameString("Graph"));
        });
    }
}