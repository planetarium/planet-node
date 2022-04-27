using Libplanet.Headless;
using Libplanet.Headless.Hosting;
using PlanetNode.Action;
using PlanetNode.GraphTypes;
using GraphQL.MicrosoftDI;
using GraphQL;
using Libplanet.Explorer.Queries;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.SystemTextJson;
using PlanetNode;
using Libplanet.Explorer.Interfaces;

// Get configuration
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("PN_");
IConfiguration config = configurationBuilder.Build();
var headlessConfig = new Configuration();
config.Bind(headlessConfig);

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddLibplanet<PlanetAction>(headlessConfig)
    .AddGraphQL(builder =>
    {
        builder
            .AddSchema<PlanetNodeSchema>()
            .AddGraphTypes(typeof(ExplorerQuery<PlanetAction>).Assembly)
            .AddSystemTextJson();
    })
    .AddSingleton<PlanetNodeSchema>()
    .AddSingleton<GraphQLHttpMiddleware<PlanetNodeSchema>>()
    .AddSingleton<IBlockChainContext<PlanetAction>, ExplorerContext>();

var app = builder.Build();
app.UseGraphQL<PlanetNodeSchema>();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQLPlayground();
});

app.Run();
