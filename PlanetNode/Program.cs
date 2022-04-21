using Libplanet.Headless;
using Libplanet.Headless.Hosting;
using PlanetNode.Action;

// Get configuration
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("PN_");
IConfiguration config = configurationBuilder.Build();
var headlessConfig = new Configuration();
config.Bind(headlessConfig);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLibplanet<PlanetAction>(headlessConfig);

var app = builder.Build();
app.Run();
