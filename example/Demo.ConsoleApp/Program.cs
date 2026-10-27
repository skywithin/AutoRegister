using Demo.ConsoleApp.Bootstrap;
using Demo.ConsoleApp.Scenarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Keep the demo output readable. Lower this to Information to see UserService's logs.
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Scans every demo layer; the library prints what it registers.
builder.Services.AddDemoConsoleApp();

using var host = builder.Build();

host.Services.GetRequiredService<LifetimeScenario>().Run();
await host.Services.GetRequiredService<UserScenario>().RunAsync();
