using Demo.ConsoleApp.Bootstrap;
using Demo.ConsoleApp.Scenarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Scans every demo layer; the library prints what it registers.
builder.Services.AddDemoConsoleApp();

using var host = builder.Build();

host.Services.GetRequiredService<LifetimeScenario>().Run();
host.Services.GetRequiredService<UserScenario>().Run();
