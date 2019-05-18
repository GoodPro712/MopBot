﻿using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MopBotTwo.TypeReaders
{
	public class SocketTextChannelArrayTypeReader : DiscordEntityArrayTypeReader<SocketTextChannel>
	{
		public override Type[] Types => new[] { typeof(IChannel[]),typeof(ITextChannel[]),typeof(SocketTextChannel[]) };

		protected Regex parseRegex;
		public override Regex ParseRegex => parseRegex ?? (parseRegex = new Regex($@"(?:(<\#\d+>|\d+)|#([\w-]+))\s*",RegexOptions.Compiled));

		public override async Task<SocketTextChannel> GetFromId(ICommandContext context,ulong id)
			=> (SocketTextChannel)await context.Guild.GetTextChannelAsync(id);
		public override async Task<SocketTextChannel> GetFromName(ICommandContext context,string name)
			=> (SocketTextChannel)(await context.Guild.GetTextChannelsAsync()).FirstOrDefault(c => MopBot.StrComparerIgnoreCase.Equals(c.Name,name));
	}
}
