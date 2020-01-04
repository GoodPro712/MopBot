﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;
using Newtonsoft.Json;
using MopBotTwo.Core.Systems.Memory;
using System.Linq;


namespace MopBotTwo.Common.Systems.Issues
{
	public class IssueServerData : ServerData
	{
		public ulong issueChannel;
		public Dictionary<IssueStatus,string> statusPrefix;
		public Dictionary<IssueStatus,string> statusText;
		public List<IssueInfo> issues;
		public uint nextIssueId;

		[JsonIgnore] public uint NextIssueId => issues!=null && issues.Any(i => i.issueId==nextIssueId) ? (nextIssueId = issues.Max(i => i.issueId)+1) : nextIssueId;
		[JsonIgnore] public IEnumerable<IssueInfo> OrderedIssues => issues.OrderBy(i => i.issueId);

		public override void Initialize(SocketGuild server)
		{
			issues = new List<IssueInfo>();

			statusPrefix = new Dictionary<IssueStatus,string> {
				{ IssueStatus.Open,		":exclamation:" },
				{ IssueStatus.Unknown,	":grey_question:" },
				{ IssueStatus.Closed,	":white_check_mark:" },
			};
			
			statusText = new Dictionary<IssueStatus,string> {
				{ IssueStatus.Open,		"To be fixed" },
				{ IssueStatus.Closed,	"Fixed for next release" },
			};
		}

		public IssueInfo NewIssue(string text,IssueStatus status = IssueStatus.Open)
		{
			var newIssue = new IssueInfo {
				issueId = NextIssueId,
				status = status,
				text = text
			};

			nextIssueId++;

			issues.Add(newIssue);

			return newIssue;
		}

		public async Task UnpublishIssue(IssueInfo issue,SocketGuild server)
		{
			if(issue.messageId==0 || issue.channelId==0) {
				return;
			}

			var oldChannel = server.GetChannel(issue.channelId);

			if(oldChannel!=null && oldChannel is SocketTextChannel oldTextChannel) {
				var oldMessage = await oldTextChannel.GetMessageAsync(issue.messageId);

				if(oldMessage!=null) {
					await oldMessage.DeleteAsync();
				}
			}
		}
		public async Task PublishIssue(IssueInfo issue,SocketTextChannel channel)
		{
			await UnpublishIssue(issue,channel.Guild);

			string text = statusText[issue.status];

			var message = await channel.SendMessageAsync($"{statusPrefix[issue.status]} - #**{issue.issueId}** - **{text}:** {issue.text}",options:MopBot.optAlwaysRetry);

			issue.messageId = message.Id;
			issue.channelId = channel.Id;
		}
		public async Task<SocketTextChannel> GetIssueChannel(MessageExt context,bool throwError = true)
		{
			var result = issueChannel!=0 ? (SocketTextChannel)MopBot.client.GetChannel(issueChannel) : null;

			if(result==null && throwError) {
				throw new BotError("Issue channel has not been set. Set it with `!issues setchannel <channel>` first.");
			}

			return result;
		}
	}
}