﻿#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Commands.Games
{
    public partial class CommandsGames
    {
        [Group("quiz", CanInvokeWithoutSubcommand = false)]
        [Description("Start a quiz!")]
        [Aliases("trivia", "q")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public class CommandsQuiz
        {
            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries")]
            [Description("Country flags quiz.")]
            [Aliases("flags")]
            public async Task CountriesQuizAsync(CommandContext ctx)
            {
                Quiz.LoadCountries();

                if (Quiz.QuizExistsInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Quiz is already running in this channel!");

                var quiz = new Quiz(ctx.Client, ctx.Channel.Id);
                await quiz.StartAsync(QuizType.Countries)
                    .ConfigureAwait(false);

                ctx.Dependencies.GetDependency<GameStatsManager>().UpdateQuizesWonForUser(ctx.User.Id);
            }
            #endregion
        }
    }
}
