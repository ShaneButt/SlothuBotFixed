using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace SlothuBot
{
    class Session
    {

        public string Boss = "";
        public int SessionLength = 1;
        public int SessionID = 1;
        public List<SocketUser> Members;
        public int Value = 0;
        public bool IsActive = false;
        public List<string> Loot = new List<string>(100);
        public DateTime LastModification = DateTime.Now;
        public SocketGuild Guild;
        public SocketUser Leader;
        public SocketCommandContext Context = null;

        private static readonly Color EmbedColor = new Color(54, 57, 63);
        private readonly Config Config = new Config();

        public Session()
        {
            IsActive = true;
        }

        public Session(SocketCommandContext Context, int SessionNum = 1, int Length = 1)
        {
            this.Context = Context;
            Leader = Context.Message.Author;
            Guild = Context.Guild;
            SessionLength = Length;
            SessionID = SessionNum;
            AddMember(Leader);
        }

        public void ChangeBoss(string BossName)
            => Boss = BossName;
        public void ChangeLength(int Length)
            => SessionLength = Length;
        public void AddMember(SocketUser Member)
            => Members.Add(Member);
        public void RemoveMember(SocketUser Member)
            => Members.Remove(Member);
        public List<SocketUser> TeamMembers()
            => Members;
        public void SetActive(bool IsActive)
            => this.IsActive = IsActive;
        public void AddLoot(string Item)
            => Loot.Add(Item);
        public void AddWealth(int Amount)
            => Value += Amount;
        
        public async Task ViewSession()
        {
            EmbedBuilder Builder = new EmbedBuilder();
            Builder.AddField("Boss", Boss.ToUpperInvariant(), true);
            Builder.AddField("Duration", SessionLength, true);
            Builder.AddField("Team", "", true); // MentionMembers() method
            Builder.AddField("Session Identifier", SessionID, true);
            Builder.AddField("All Loot", "", true); // ConcatLoot() method
            Builder.AddField("Wealth", Value, true);
            Builder.AddField("Status", IsActive.ToString(), false);
            Builder.WithColor(EmbedColor);
            Builder.WithThumbnailUrl(Context.User.GetDefaultAvatarUrl());

            Embed BuiltEmbed = Builder.Build();
            await Context.Channel.SendMessageAsync("", false, BuiltEmbed);
        }

        public string Serialise()
            => JsonConvert.SerializeObject(this);
        
        public void Save()
        {
            string UpdatedJSON = Serialise();
            /*
             * ExternalModule Module = new ExternalModule()
             * Module.WriteFile(FilePath, UpdatedJSON);
             */
        }

        public string MentionMembers()
        {
            string _team = "";
            foreach(SocketUser user in Members)
            {
                _team += user.Mention + "\n";
            }
            return _team;
        }

        public string ConcatLoot()
        {
            if (Loot.Count < 1) return "N/A";
            string _loot = "";
            foreach(string item in Loot)
            {
                _loot += item + "\n";
            }
            return _loot;
        }
    }
}
