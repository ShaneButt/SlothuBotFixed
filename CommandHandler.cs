using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using Discord;
using System.Diagnostics;
using Discord.Rest;
using System.Collections.Generic;

namespace SlothuBot
{
    class CommandHandler
    {
        private readonly string Prefix = "~";
        private DiscordSocketClient Client;
        private CommandService Service;
        private DateTime Start;

        public CommandHandler(DiscordSocketClient client)
        {
            Client = client;
            Environment.SetEnvironmentVariable("prefix", Prefix);
            CommandServiceConfig Config = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                SeparatorChar = ',',
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Debug
            };
            Service = new CommandService(Config);
            Service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            Client.MessageReceived += HandleCommandAsync;
            Client.Ready += HandleReadyAsync;
            Client.Log += HandleLogEvent;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            SocketUserMessage msg = message as SocketUserMessage;
            if (msg == null || msg.Author.IsBot) return;

            SocketCommandContext context = new SocketCommandContext(Client, msg);
            int argPos = 0;
            if(msg.HasStringPrefix(Prefix, ref argPos))
            {
                IResult res = await Service.ExecuteAsync(context, argPos, null);
                if (!res.IsSuccess && res.Error != CommandError.UnknownCommand)
                    Console.WriteLine(res.Error);
            }
        }

        private async Task HandleReadyAsync()
        {
            RestApplication info = await Client.GetApplicationInfoAsync();
            IReadOnlyCollection<SocketGuild> Guilds = Client.Guilds;
            foreach(SocketGuild Guild in Guilds)
            {

            }
            Environment.SetEnvironmentVariable("startTime", DateTime.Now.ToString());
            await Client.SetGameAsync($"You on {Guilds.Count} servers\nPrefix {Environment.GetEnvironmentVariable("prefix")}", 
                null, 
                ActivityType.Watching
            );
        }

        private async Task HandleLogEvent(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Source);
        }
    }
}
