﻿#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class VotingModule : TheGodfatherBaseModule
    {
        [Command("vote")]
        [Description("Vote for an option in the current running poll.")]
        [UsageExample("!poll Do you vote for User1 or User2?")]
        public async Task VoteAsync(CommandContext ctx,
                                   [Description("Option to vote for.")] int option)
        {
            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.Running || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");

            if (!poll.IsValidVote(option))
                throw new CommandFailedException($"Invalid poll option. Valid range: [1, {poll.OptionCount}].");

            if (poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have already voted in this poll!");

            poll.VoteFor(ctx.User.Id, option);
            await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} voted for: **{poll.OptionWithId(option)}** in poll: {Formatter.Italic($"\"{poll.Question}\"")}")
                .ConfigureAwait(false);
        }
    }
}