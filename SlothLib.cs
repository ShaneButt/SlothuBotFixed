using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.IO;
using Discord;
using System.Globalization;
using System.Linq;
using Discord.Rest;
using Discord.Commands;
using Newtonsoft.Json;

namespace SlothuBot
{
    class SlothLib
    {

        private static readonly Config Config = new Config();
        private static readonly string LocalDir = Config.BotFilePath;
        private static readonly string GuildDir = Config.GuildFilePath;
        private static readonly string RebootScript = LocalDir + "reboot.bat";

        public SlothLib() { }

        public async Task Logout(DiscordSocketClient Client)
            => await Client.LogoutAsync();

        internal class FileHandler
        {
            public string CreateDirectory(ulong GuildId, string FolderName)
            {
                string Dir = GuildDir + GuildId.ToString();
                string PathSource = Path.Combine(Dir, FolderName);
                if (Directory.Exists(PathSource)) return PathSource;
                Directory.CreateDirectory(PathSource);
                return PathSource;
            }

            public string CreateFile(string Source, string FileName)
            {
                string Dir = GuildDir + Source + @"\" + FileName; // D:\DiscordBotGuilds\ + GuildId\Folder? + \ + FileName
                if (File.Exists(Dir)) return Dir;
                FileStream fs = File.Create(Dir);
                fs.Close();
                return Dir;
            }

            public async Task<string> ReadFile(string FilePath)
            {
                string content;
                FileStream fs = File.OpenRead(FilePath);
                byte[] result = new byte[fs.Length];
                await fs.ReadAsync(result, 0, (int)fs.Length);
                content = Encoding.ASCII.GetString(result);
                fs.Close();
                return content;
            }

            public void WriteFile(string FilePath, string Content)
                => File.WriteAllText(FilePath, Content);

            public void AppendFile(string FilePath, string Content)
                => File.AppendAllText(FilePath, Content);
        }

        internal class GuildHandler
        {
            public SocketChannel DefaultChannel(SocketGuild Guild)
            {
                SocketChannel DefaultOrOldest = Guild.DefaultChannel;
                foreach(SocketChannel channel in Guild.Channels)
                {
                    if(DefaultOrOldest == null)
                    {
                        DefaultOrOldest = channel;
                    }
                    else
                    {
                        if(channel.CreatedAt.Ticks < DefaultOrOldest.CreatedAt.Ticks)
                        {
                            DefaultOrOldest = channel;
                        }
                    }
                }
                return DefaultOrOldest;
            }

            public void InitialiseGuildFiles(SocketGuild Guild)
            {
                ulong GuildId = Guild.Id;
                FileHandler file = new FileHandler();
                string[] Folders = {"Bossing", "CustomResponses", "Reminders"};
                string[] Files =
                {
                    "MorningMessage.txt",
                    "EveningMessage.txt",
                    "MorningTime.txt",
                    "EveningTime.txt",
                    "DefaultChannel.txt"
                };
                foreach(string Folder in Folders)
                {
                    file.CreateDirectory(GuildId, Folder);
                }
                foreach(string File in Files)
                {
                    file.CreateFile(GuildDir + $@"{GuildId}\", File);
                }
            }

            public int OnlineMemberCount(SocketGuild Guild)
            {
                int Online = 0;
                foreach(SocketGuildUser guildUser in Guild.Users)
                {
                    if(guildUser.Status != UserStatus.Offline && guildUser.Status != UserStatus.Invisible)
                    {
                        Online++;
                    }
                }
                return Online;
            }

            public int HumanCount(SocketGuild Guild)
            {
                int Humans = 0;
                foreach (SocketGuildUser guildUser in Guild.Users)
                {
                    if(!(guildUser.IsBot))
                    {
                        Humans++;
                    }
                }
                return Humans;
            }

        }

        internal class SessionHandler
        {
            public Session Create(SocketCommandContext Context)
            {
                ulong GuildID = Context.Guild.Id;
                FileHandler file = new FileHandler();
                string SessionsFolder = file.CreateDirectory(GuildID, "Sessions");
                int SessionCount = Directory.GetFiles(SessionsFolder).Length + 1;
                Session session = new Session(Context, SessionCount);
                string JSON = JsonConvert.SerializeObject(session);
                file.CreateFile(SessionsFolder, $"session{SessionCount}.json");
                file.WriteFile(SessionsFolder + $@"\session{SessionCount}.json", JSON);
                return session;
            }

            public Session GetSession(SocketCommandContext Context, int ID)
            {
                string FilePath = GuildDir + $@"{Context.Guild.Id}\Bossing\Sessions\session{ID}";
                bool DoesExist = File.Exists(FilePath);
                if (!DoesExist) return null;

                FileHandler file = new FileHandler();
                string JSON = file.ReadFile(FilePath).GetAwaiter().GetResult();
                Session session = new Session();
                session = JsonConvert.DeserializeObject<Session>(JSON);
                return session;
            }

            public string SessionToJSON(SocketCommandContext Context, int ID)
                => JsonConvert.SerializeObject( GetSession(Context, ID) );

            public List<Session> GetAllSessions(SocketCommandContext Context)
            {
                string FilePath = GuildDir + $@"{Context.Guild.Id}\Bossing\Sessions";
                int NumSessions = Directory.GetFiles(FilePath).Count();
                List<Session> Sessions = new List<Session>();
                FileHandler file = new FileHandler();
                for(int i = 1; i <= NumSessions; i++)
                {
                    string NewPath = FilePath + $@"\session{i}.json";
                    string SessionJSON = file.ReadFile(NewPath).GetAwaiter().GetResult();
                    Session session = new Session();
                    session = JsonConvert.DeserializeObject<Session>(SessionJSON);
                    Sessions.Add(session);
                }
                return Sessions;
            }

            public bool UserIsInSession(SocketCommandContext Context, SocketUser Caller)
            {
                bool isInSession = false;
                List<Session> Sessions = GetAllSessions(Context);
                foreach (Session session in Sessions)
                {
                    if (session.TeamMembers().Contains(Caller))
                    {
                        isInSession = true;
                        break;
                    }
                }
                return isInSession;
            }
        }

        internal class BotHandler
        {

        }
    }
}
