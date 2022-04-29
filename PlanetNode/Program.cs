using PlanetNode.Action;
using PlanetNode.GraphTypes;
using GraphQL.MicrosoftDI;
using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.SystemTextJson;
using PlanetNode;
using Libplanet.Action;
using Libplanet.Explorer.Interfaces;
using Libplanet.Explorer.Queries;
using Libplanet.Headless;
using Libplanet.Headless.Hosting;
using System.Net;

// Get configuration
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("PN_");
IConfiguration config = configurationBuilder.Build();
var headlessConfig = new Configuration();
config.Bind(headlessConfig);

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddLibplanet<PolymorphicAction<PlanetAction>>(headlessConfig)
    .AddGraphQL(builder =>
    {
        builder
            .AddSchema<PlanetNodeSchema>()
            .AddGraphTypes(typeof(ExplorerQuery<PolymorphicAction<PlanetAction>>).Assembly)
            .AddSystemTextJson();
    })
    .AddSingleton<PlanetNodeSchema>()
    .AddSingleton<PlanetNodeMutation>()
    .AddSingleton<GraphQLHttpMiddleware<PlanetNodeSchema>>()
    .AddSingleton<IBlockChainContext<PolymorphicAction<PlanetAction>>, ExplorerContext>();

if (
    headlessConfig.GraphQLHost is { } graphqlHost &&
    headlessConfig.GraphQLPort is { } graphqlPort
)
{
    builder.WebHost
        .ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Parse(graphqlHost), graphqlPort);
        });
}

var app = builder.Build();
app.UseGraphQL<PlanetNodeSchema>();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQLPlayground();
});

app.Run();
