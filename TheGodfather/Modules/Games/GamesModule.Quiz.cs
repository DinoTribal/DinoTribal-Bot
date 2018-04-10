﻿#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz"), Module(ModuleType.Games)]
        [Description("List all available quiz categories.")]
        [Aliases("trivia", "q")]
        [UsageExample("!game quiz")]
        [ListeningCheck]
        public partial class QuizModule : TheGodfatherBaseModule
        {

            public QuizModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "You need to specify a quiz type!\n\nAvailable quiz categories:\n" +
                    $"- {Formatter.Bold("capitals")}" +
                    $"- {Formatter.Bold("countries")}",
                    ":information_source:"
                ).ConfigureAwait(false);
            }


            #region COMMAND_QUIZ_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!game quiz stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopQuizPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Top players in Quiz:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
