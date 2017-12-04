﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public sealed class GameStats
    {
        [JsonProperty("duelswon")]
        public uint DuelsWon { get; internal set; }

        [JsonProperty("duelslost")]
        public uint DuelsLost { get; internal set; }

        [JsonIgnore]
        public uint DuelWinPercentage
        {
            get {
                if (DuelsWon + DuelsLost == 0)
                    return 0;
                return (uint)Math.Round((double)DuelsWon / (DuelsWon + DuelsLost) * 100);
            }
            internal set { }
        }

        [JsonProperty("hangmanwon")]
        public uint HangmanWon { get; internal set; }

        [JsonProperty("nunchiswon")]
        public uint NunchiGamesWon { get; internal set; }

        [JsonProperty("quizeswon")]
        public uint QuizesWon { get; internal set; }

        [JsonProperty("raceswon")]
        public uint RacesWon { get; internal set; }

        [JsonProperty("tttwon")]
        public uint TTTWon { get; internal set; }

        [JsonProperty("tttlost")]
        public uint TTTLost { get; internal set; }

        [JsonIgnore]
        public uint TTTWinPercentage
        {
            get {
                if (TTTWon + TTTLost == 0)
                    return 0;
                return (uint)Math.Round((double)TTTWon / (TTTWon + TTTLost) * 100);
            }
            internal set { }
        }


        public string DuelStatsString() => $"W: {DuelsWon} L: {DuelsLost} ({DuelWinPercentage}%)";
        public string TTTStatsString() => $"W: {TTTWon} L: {TTTLost} ({TTTWinPercentage}%)";
        public string NunchiStatsString() => $"{NunchiGamesWon}";
        public string QuizStatsString() => $"{QuizesWon}";
        public string RaceStatsString() => $"{RacesWon}";
        public string HangmanStatsString() => $"{HangmanWon}";

        public DiscordEmbedBuilder GetEmbeddedStats()
        {
            var eb = new DiscordEmbedBuilder() { Color = DiscordColor.Chartreuse };
            eb.AddField("Duel stats", DuelStatsString());
            eb.AddField("Tic-Tac-Toe stats", TTTStatsString());
            eb.AddField("Nunchi games won", NunchiStatsString(), inline: true);
            eb.AddField("Quizzes won", QuizStatsString(), inline: true);
            eb.AddField("Races won", RaceStatsString(), inline: true);
            eb.AddField("Hangman games won", HangmanStatsString(), inline: true);
            return eb;
        }

    }
}
