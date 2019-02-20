using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace SlothuBot
{
    class Program
    {
        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient CLIENT;
        private CommandHandler HANDLER;
        private Config CONFIG = new Config();
        public DateTime START = DateTime.Now;

        public async Task StartAsync()
        {
            string Token;
            FileStream TokenFile = File.Open(CONFIG.BotFilePath + "token.txt", FileMode.Open, FileAccess.Read);
            byte[] Result = new byte[TokenFile.Length];
            await TokenFile.ReadAsync(Result, 0, (int)TokenFile.Length);
            Token = Encoding.ASCII.GetString(Result);
            CLIENT = new DiscordSocketClient();
            long Ticks = DateTime.Now.Ticks;
            await CLIENT.LoginAsync(
                tokenType: TokenType.Bot,
                token: Token
            );
            Console.WriteLine("-----------------------------------------------------" +
                "\nTime taken to login: {0:n3}s ({1}ms)" +
                "\n-----------------------------------------------------",
                (DateTime.Now.Ticks - Ticks) / 10 / 1000 / 1000,
                (DateTime.Now.Ticks - Ticks) / 10 / 1000);
            await CLIENT.StartAsync();
            HANDLER = new CommandHandler(CLIENT);
            await Task.Delay(-1);
        }
    }
}
