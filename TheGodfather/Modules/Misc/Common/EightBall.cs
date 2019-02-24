﻿#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Misc.Common
{
    public static class EightBall
    {
        private static ImmutableArray<string> _regularAnswers = new string[] {
            "Definitely NO.",
            "Are you crazy? No.",
            "As I see it, no.",
            "Unlikely.",
            "No.",
            "I don't think so.",
            "I need time to think, ask me later.",
            "I think so.",
            "Yes.",
            "As I see it, yes.",
            "Hell yeah!",
            "Most likely.",
            "Definitely YES.",
            "More than you can imagine."
        }.ToImmutableArray();

        private static ImmutableArray<string> _timeAnswers = new string[] {
            "Right now.",
            "Soon™",
            "In 5 minutes.",
            "In 30 minutes.",
            "In 1 hour.",
            "Tomorrow.",
            "Next month.",
            "Next year.",
            "Never.",
            "When I grow a beard."
        }.ToImmutableArray();

        private static ImmutableArray<string> _quantityAnswers = new string[] {
            "None.",
            "One.",
            "A few.",
            "Five.",
            "A dozen.",
            "One hundred.",
            "One thousand.",
            "It is impossible to count...",
        }.ToImmutableArray();


        public static string GenerateAnswer(string question, IEnumerable<DiscordMember> members)
        {
            if (question.StartsWith("when", StringComparison.InvariantCultureIgnoreCase) ||
                question.StartsWith("how long", StringComparison.InvariantCultureIgnoreCase))
                return GFRandom.Generator.ChooseRandomElement(_timeAnswers);
            if (question.StartsWith("who", StringComparison.InvariantCultureIgnoreCase))
                return GFRandom.Generator.ChooseRandomElement(members).DisplayName;
            if (question.StartsWith("how much", StringComparison.InvariantCultureIgnoreCase) || 
                question.StartsWith("how many", StringComparison.InvariantCultureIgnoreCase))
                return GFRandom.Generator.ChooseRandomElement(_quantityAnswers);
            else
                return GFRandom.Generator.ChooseRandomElement(_regularAnswers);
        }
    }
}
