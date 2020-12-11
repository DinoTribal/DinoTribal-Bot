﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ClientErrored)]
        public static Task ClientErrorEventHandlerAsync(TheGodfatherShard shard, ClientErrorEventArgs e)
        {
            Exception ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException ?? ex;

            LogExt.Error(shard.Id, ex, "Client errored!");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static Task GuildAvailableEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Available: {AvailableGuild}", e.Guild);
            GuildConfigService gcs = shard.Services.GetRequiredService<GuildConfigService>();
            return gcs.IsGuildRegistered(e.Guild.Id) ? Task.CompletedTask : gcs.RegisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildUnavailable)]
        public static Task GuildUnvailableEventHandlerAsync(TheGodfatherShard shard, GuildDeleteEventArgs e)
        {
            LogExt.Warning(shard.Id, "Unvailable: {UnvailableGuild}", e.Guild);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildDownloadCompleted)]
        public static Task GuildDownloadCompletedEventHandlerAsync(TheGodfatherShard shard, GuildDownloadCompletedEventArgs e)
        {
            LogExt.Information(shard.Id, "All guilds for this shard are now downloaded ({Count} total)", e.Guilds.Count);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildCreated)]
        public static async Task GuildCreateEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Joined {NewGuild}", e.Guild);

            if (shard.Services.GetRequiredService<BlockingService>().IsGuildBlocked(e.Guild.Id)) {
                LogExt.Information(shard.Id, "{Guild} is blocked. Leaving...", e.Guild);
                await e.Guild.LeaveAsync();
                return;
            }

            await shard.Services.GetRequiredService<GuildConfigService>().RegisterGuildAsync(e.Guild.Id);

            DiscordChannel defChannel = e.Guild.GetDefaultChannel();
            if (!defChannel.PermissionsFor(e.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
                return;

            string prefix = shard.Services.GetRequiredService<BotConfigService>().CurrentConfiguration.Prefix;
            string owners = shard.Client.CurrentApplication.Owners.Select(o => o.ToDiscriminatorString()).Humanize(", ");
            await defChannel.EmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{Emojis.SmallBlueDiamond} The default prefix for commands is {Formatter.Bold(prefix)}, but it can be changed " +
                $"via {Formatter.Bold("prefix")} command.\n" +
                $"{Emojis.SmallBlueDiamond} I advise you to run the configuration wizard for this guild in order to quickly configure " +
                $"functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("config setup")} command.\n" +
                $"{Emojis.SmallBlueDiamond} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to " +
                $"read the documentation @ https://github.com/ivan-ristovic/the-godfather \n" +
                $"{Emojis.SmallBlueDiamond} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} " +
                $"command in order to send a message to the bot owners ({owners}). Alternatively, you can create an issue on " +
                $"GitHub or join WorldMafia Discord server for quick support (https://worldmafia.net/discord)."
                , Emojis.Wave
            );
        }

        [AsyncEventListener(DiscordEventType.SocketOpened)]
        public static Task SocketOpenedEventHandlerAsync(TheGodfatherShard shard, SocketEventArgs _)
        {
            LogExt.Debug(shard.Id, "Socket opened");
            shard.Services.GetRequiredService<BotActivityService>().ShardUptimeInformation[shard.Id].SocketStartTime = DateTimeOffset.Now;
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.SocketClosed)]
        public static Task SocketClosedEventHandlerAsync(TheGodfatherShard shard, SocketCloseEventArgs e)
        {
            LogExt.Debug(shard.Id, "Socket closed with code {Code}: {Message}", e.CloseCode, e.CloseMessage);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.SocketErrored)]
        public static Task SocketErroredEventHandlerAsync(TheGodfatherShard shard, SocketErrorEventArgs e)
        {
            LogExt.Debug(shard.Id, e.Exception, "Socket errored");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UnknownEvent)]
        public static Task UnknownEventHandlerAsync(TheGodfatherShard shard, UnknownEventArgs e)
        {
            LogExt.Error(shard.Id, new[] { "Unknown event ({UnknownEvent}) occured:", "{@UnknownEventJson}" }, e.EventName, e.Json);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UserUpdated)]
        public static Task UserUpdatedEventHandlerAsync(TheGodfatherShard shard, UserUpdateEventArgs e)
        {
            LogExt.Information(shard.Id, new[] { "Bot updated:", "{@BotUserBefore}", "{@BotUserAfter}" }, e.UserBefore, e.UserAfter);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.UserSettingsUpdated)]
        public static Task UserSettingsUpdatedEventHandlerAsync(TheGodfatherShard shard, UserSettingsUpdateEventArgs e)
        {
            LogExt.Information(shard.Id, "Bot settings updated: {@BotUser}", e.User);
            return Task.CompletedTask;
        }
    }
}
