using Cocona;
using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.SystemTextJson;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Explorer.Interfaces;
using Libplanet.Explorer.Queries;
using Libplanet.Extensions.Cocona.Commands;
using Libplanet.Headless;
using Libplanet.Headless.Hosting;
using PlanetNode;
using PlanetNode.Action;
using PlanetNode.GraphTypes;
using Serilog;
using System.Collections.Immutable;
using System.Net;

var app = CoconaApp.Create();

app.AddCommand(() =>
{
    // Get configuration
    string configPath = Environment.GetEnvironmentVariable("PN_CONFIG_FILE") ?? "appsettings.json";

    var configurationBuilder = new ConfigurationBuilder()
        .AddJsonFile(configPath)
        .AddEnvironmentVariables("PN_");
    IConfiguration config = configurationBuilder.Build();

    var loggerConf = new LoggerConfiguration()
       .ReadFrom.Configuration(config);
    Log.Logger = loggerConf.CreateLogger();

    var headlessConfig = new Configuration();
    config.Bind(headlessConfig);

    var builder = WebApplication.CreateBuilder(args);
    builder.Services
        .AddLibplanet(
            headlessConfig,
            new PolymorphicAction<PlanetAction>[]
            {
                new InitializeStates(
                    new Dictionary<Address, FungibleAssetValue>
                    {
                        [new Address("019101FEec7ed4f918D396827E1277DEda1e20D4")] = Currencies.PlanetNodeGold * 1000,
                    }
                ),
            },
            ImmutableHashSet.Create(Currencies.PlanetNodeGold)
        )
        .AddGraphQL(builder =>
        {
            builder
                .AddSchema<PlanetNodeSchema>()
                .AddGraphTypes(typeof(ExplorerQuery<PolymorphicAction<PlanetAction>>).Assembly)
                .AddGraphTypes(typeof(PlanetNodeQuery).Assembly)
                .AddUserContextBuilder<ExplorerContextBuilder>()
                .AddSystemTextJson();
        })
        .AddCors()
        .AddSingleton<PlanetNodeSchema>()
        .AddSingleton<PlanetNodeQuery>()
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

    using WebApplication app = builder.Build();
    app.UseCors(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGraphQLPlayground();
    });
    app.UseGraphQL<PlanetNodeSchema>();

    app.Run();
});

app.AddSubCommand("key", x =>
{
    x.AddCommands<KeyCommand>();
});

app.Run();
