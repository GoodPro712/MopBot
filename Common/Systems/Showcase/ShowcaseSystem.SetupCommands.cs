﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using MopBot.Extensions;
using MopBot.Core.Systems.Permissions;

#pragma warning disable CS1998 //Async method lacks 'await' operators and will run synchronously
namespace MopBot.Common.Systems.Showcase
{
	public partial class ShowcaseSystem
	{
		[Command("removechannel")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		public async Task RemoveChannel(SocketTextChannel channel)
		{
			var showcaseData = Context.server.GetMemory().GetData<ShowcaseSystem,ShowcaseServerData>();

			showcaseData.RemoveChannel(channel.Id);
		}

		[Command("setupchannel showcase")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		public Task SetupChannelShowcase(SocketTextChannel channel,[Remainder]SocketRole[] rewardRoles = null) => SetupChannelShowcase(channel,null,0,rewardRoles);

		[Command("setupchannel showcase")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		[Priority(10)]
		public async Task SetupChannelShowcase(SocketTextChannel channel,SocketTextChannel spotlightChannel,uint spotlightScore,[Remainder]SocketRole[] rewardRoles = null)
		{
			if(channel==spotlightChannel) {
				throw new BotError("'channel' can't be the same value as 'spotlightChannel'.");
			}

			var showcaseData = Context.server.GetMemory().GetData<ShowcaseSystem,ShowcaseServerData>();
			var channelId = channel.Id;

			if(spotlightChannel!=null && !showcaseData.ChannelIs<SpotlightChannel>(spotlightChannel)) {
				throw new BotError($"Channel <#{spotlightChannel.Id}> is not a spotlight channel. Setup it as one first before setting up showcase channels for it.");
			}

			ArrayUtils.ModifyOrAddFirst(ref showcaseData.showcaseChannels,c => c.id==channelId,() => new ShowcaseChannel(),c => {
				c.id = channel.Id;
				c.spotlightChannel = spotlightChannel==null ? 0 : spotlightChannel.Id;
				c.minSpotlightScore = spotlightScore;
				c.rewardRoles = rewardRoles==null || rewardRoles.Length==0 ? null : rewardRoles.SelectIgnoreNull(role => role.Id).ToList();
			},true);
		}

		[Command("setupchannel spotlight")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		public async Task SetupChannelSpotlight(SocketTextChannel channel,[Remainder]SocketRole[] rewardRoles)
		{
			var showcaseData = Context.server.GetMemory().GetData<ShowcaseSystem,ShowcaseServerData>();
			var channelId = channel.Id;

			ArrayUtils.ModifyOrAddFirst(ref showcaseData.spotlightChannels,c => c.id==channelId,() => new SpotlightChannel(),c => {
				c.id = channel.Id;
				c.rewardRoles = rewardRoles==null || rewardRoles.Length==0 ? null : rewardRoles.SelectIgnoreNull(role => role.Id).ToList();
			},true);
		}

		[Command("setemote")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		public async Task SetEmote(EmoteType type,IEmote emote)
		{
			var showcaseData = Context.server.GetMemory().GetData<ShowcaseSystem,ShowcaseServerData>();
			var dict = showcaseData.emotes ??= new Dictionary<EmoteType,string>();

			dict[type] = emote?.ToString();
		}

		[Command("resetemote")]
		[RequirePermission(SpecialPermission.Owner,"showcasesystem.configure")]
		public Task SetEmote(EmoteType type) => SetEmote(type,null);
	}
}
