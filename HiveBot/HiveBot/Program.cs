using System;
using System.Threading.Tasks;
using HiveBot.Commands;
using HiveBot.Configuration;
using HiveBot.Responders;
using HiveBot.Services;
using HiveBot.Services.AgeOfEmpires;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Serilog;

namespace HiveBot
{
    class Program
    {
        public static Task Main(string[] args)
        {
            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByExcluding(
                        "Contains(SourceContext, 'Quartz') and (@Level = 'Information')");
                    lc.WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .WriteTo.Logger(lc =>
                {
                    lc.WriteTo.File(path: "hivebot.txt",
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .CreateLogger();

            return CreateHostBuilder(args).RunConsoleAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .AddDiscordService(services =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                return configuration.GetValue<string>("discord:token") ??
                       throw new InvalidOperationException
                       (
                           "Failed to read Discord configuration, bot token not found in appsettings.json."
                       );
            })
            .ConfigureServices((_, services) =>
            {
                // Add configuration
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddCommandLine(args)
                    .AddUserSecrets<Program>()
                    .Build();
                services.AddSingleton<IConfiguration>(config);

                // Add Discord config
                services.AddOptions<DiscordConfiguration>()
                    .Bind(config.GetSection("discord"))
                    .Validate(x => x.Token != null);

                // Set intents
                services.Configure<DiscordGatewayClientOptions>(options =>
                {
                    options.Intents |= GatewayIntents.GuildVoiceStates;
                });
                
                // Add services
                services.AddSingleton<UserChannelResolverService>();
                services.AddHttpClient<AgeOfEmpiresService>();
                
                // Add Discord commands
                services
                    .AddDiscordCommands(true)
                    .AddResponder<SlashCommandConfigurator>()
                    .AddResponder<ChannelPresenceTracker>()
                    .AddCommandGroup<SessionCommands>()
                    .AddCommandGroup<MetaCommands>()
                    .AddCommandGroup<AgeOfEmpiresCommands>();
            })
            .UseSerilog();
    }
}