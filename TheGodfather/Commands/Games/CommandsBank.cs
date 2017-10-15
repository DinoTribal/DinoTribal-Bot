﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    [Group("bank", CanInvokeWithoutSubcommand = true)]
    [Description("$$$")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsBank
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await Status(ctx);
        }


        #region COMMAND_GRANT
        [Command("grant")]
        [Description("Magically give funds to a user.")]
        [Aliases("give")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task Register(CommandContext ctx,
                                  [Description("User.")] DiscordUser u = null,
                                  [Description("Ammount.")] int ammount = 0)
        {
            if (u == null || ammount <= 0 || ammount > 1000)
                throw new InvalidCommandUsageException("Invalid user or ammount.");

            ctx.Dependencies.GetDependency<BankManager>().IncreaseBalance(u.Id, ammount);
            await ctx.RespondAsync($"User {Formatter.Bold(u.Username)} won {Formatter.Bold(ammount.ToString())} credits on a lottery! (seems legit)");
        }
        #endregion

        #region COMMAND_REGISTER
        [Command("register")]
        [Description("Create an account in WM bank.")]
        [Aliases("r", "signup", "activate")]
        public async Task Register(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<BankManager>().Accounts.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("You already own an account in WM bank!");

            if (ctx.Dependencies.GetDependency<BankManager>().OpenAccount(ctx.User.Id))
                await ctx.RespondAsync($"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get 25 credits for free.");
            else
                throw new CommandFailedException("Account opening failed.");
        }
        #endregion

        #region COMMAND_STATUS
        [Command("status")]
        [Description("View your account balance.")]
        [Aliases("s", "balance")]
        public async Task Status(CommandContext ctx)
        {
            var accounts = ctx.Dependencies.GetDependency<BankManager>().Accounts;
            int? ammount = null;
            if (accounts.ContainsKey(ctx.User.Id))
                ammount = accounts[ctx.User.Id];

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Account balance for {ctx.User.Username}",
                Description = $"{Formatter.Bold(ammount != null ? ammount.ToString() : "No existing account!")}",
                Color = DiscordColor.Yellow
            });
        }
        #endregion

        #region COMMAND_TOP
        [Command("top")]
        [Description("Print the richest users.")]
        [Aliases("leaderboard")]
        public async Task Top(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder() { Title = "WEALTHIEST PEOPLE IN WM BANK:" };

            int i = 10;
            foreach (var pair in ctx.Dependencies.GetDependency<BankManager>().Accounts.ToList().OrderBy(key => key.Value)) {
                if (i-- != 0) {
                    var username = ctx.Guild.GetMemberAsync(pair.Key).Result.Username;
                    embed.AddField(username, pair.Value.ToString(), inline: true);
                }
            }

            await ctx.RespondAsync(embed: embed);
        }
        #endregion

        #region COMMAND_TRANSFER
        [Command("transfer")]
        [Description("Transfer funds from one account to another.")]
        [Aliases("lend")]
        public async Task Transfer(CommandContext ctx,
                                  [Description("User to send credits to.")] DiscordUser u = null,
                                  [Description("Ammount.")] int ammount = 0)
        {
            if (u == null)
                throw new InvalidCommandUsageException("Account to transfer the credits to is missing.");

            var accounts = ctx.Dependencies.GetDependency<BankManager>().Accounts;

            if (!accounts.ContainsKey(ctx.User.Id) || !accounts.ContainsKey(u.Id))
                throw new CommandFailedException("One or more accounts not found in the bank.", new KeyNotFoundException());

            if (ammount <= 0 || accounts[ctx.User.Id] < ammount)
                throw new CommandFailedException("Invalid ammount (check your funds).", new ArgumentOutOfRangeException());

            ctx.Dependencies.GetDependency<BankManager>().RetrieveCredits(ctx.User.Id, ammount);
            ctx.Dependencies.GetDependency<BankManager>().IncreaseBalance(u.Id, ammount);

            await ctx.RespondAsync($"Transfer from {ctx.User.Mention} to {u.Mention} is complete.");
        }
        #endregion
    }
}
