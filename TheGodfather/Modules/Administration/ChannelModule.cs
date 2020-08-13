﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Administration
{
    [Group("channel"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("channels", "chn", "ch", "c")]
    [RequireGuild]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed partial class ChannelModule : TheGodfatherModule
    {
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-chn-info")] DiscordChannel? channel = null)
            => this.InfoAsync(ctx, channel);


        #region clone
        [Command("clone"), UsesInteractivity]
        [Aliases("copy", "cp", "cln")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CloneAsync(CommandContext ctx,
                                    [Description("desc-chn-clone")] DiscordChannel channel,
                                    [RemainingText, Description("desc-chn-clone-name")] string? name = null)
        {
            DiscordChannel cloned = await channel.CloneAsync(ctx.BuildInvocationDetailsString());

            await this.CheckPotentialChannelNameAsync(ctx, name, false, channel.Type == ChannelType.Text);
            if (!string.IsNullOrWhiteSpace(name)) 
                await cloned.ModifyAsync(new Action<ChannelEditModel>(m => m.Name = name));

            await ctx.InfoAsync(this.ModuleColor, "fmt-chn-clone", Formatter.Bold(channel.Name), Formatter.Bold(cloned.Name));
        }
        #endregion

        #region category
        [Command("category"), UsesInteractivity]
        [Aliases("addcategory", "cat", "c", "cc", "+category", "+cat", "+c", "<c", "<<c")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [RemainingText, Description("desc-cat-name")] string name)
        {
            await this.CheckPotentialChannelNameAsync(ctx, name);
            DiscordChannel c = await ctx.Guild.CreateChannelCategoryAsync(name, reason: ctx.BuildInvocationDetailsString());
            await ctx.InfoAsync(this.ModuleColor, "fmt-chn-category", Formatter.Bold(c.Name));
        }
        #endregion

        #region text
        [Command("text"), Priority(0)]
        [Aliases("addtext", "addtxt", "txt", "ctxt", "ct", "+", "+txt", "+t", "<t", "<<t")]
        [RequirePermissions(Permissions.ManageChannels)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("chn-parent")] DiscordChannel parent,
                                          [Description("chn-name")] string name,
                                          [Description("chn-nsfw")] bool nsfw = false)
           => this.CreateTextChannelAsync(ctx, name, parent, nsfw);

        [Command("text"), Priority(1)]
        public Task CreateTextChannelAsync(CommandContext ctx,
                                          [Description("chn-name")] string name,
                                          [Description("chn-nsfw")] bool nsfw = false,
                                          [Description("chn-parent")] DiscordChannel? parent = null)
           => this.CreateTextChannelAsync(ctx, name, parent, nsfw);

        [Command("text"), Priority(2), UsesInteractivity]
        public async Task CreateTextChannelAsync(CommandContext ctx,
                                                [Description("chn-name")] string name,
                                                [Description("chn-parent")] DiscordChannel? parent = null,
                                                [Description("chn-nsfw")] bool nsfw = false)
        {
            await this.CheckPotentialChannelNameAsync(ctx, name, true, true);

            if (parent is { } && parent.Type != ChannelType.Category)
                throw new CommandFailedException(ctx, "cmd-err-chn-parent");

            DiscordChannel c = await ctx.Guild.CreateTextChannelAsync(name, parent, nsfw: nsfw, reason: ctx.BuildInvocationDetailsString());
            await ctx.InfoAsync(this.ModuleColor, "fmt-chn-text", c.Mention);
        }
        #endregion

        #region COMMAND_CHANNEL_CREATEVOICE
        [Command("createvoice"), Priority(2), UsesInteractivity]
        [Description("Create new voice channel. You can also specify channel parent, user limit and bitrate.")]
        [Aliases("addvoice", "addv", "createv", "cvoice", "cv", "+voice", "+v", "<v", "<<v")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CreateVoiceChannelAsync(CommandContext ctx,
                                                 [Description("Name for the channel.")] string name,
                                                 [Description("Parent category.")] DiscordChannel parent = null,
                                                 [Description("User limit.")] int? userlimit = null,
                                                 [Description("Bitrate.")] int? bitrate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing channel name.");

            if (name.Length > 100)
                throw new InvalidCommandUsageException("Name must be shorter than 100 characters.");

            if (!(parent is null) && parent.Type != ChannelType.Category)
                throw new CommandFailedException("Channel parent must be a category!");

            if (ctx.Guild.Channels.Select(kvp => kvp.Value).Any(chn => string.Compare(name, chn.Name, true) == 0)) {
                if (!await ctx.WaitForBoolReplyAsync("A channel with that name already exists. Continue?"))
                    return;
            }

            DiscordChannel c = await ctx.Guild.CreateVoiceChannelAsync(name, parent, bitrate, userlimit, reason: ctx.BuildInvocationDetailsString());
            await this.InformAsync(ctx, $"Successfully created voice channel: {Formatter.Bold(c.Name)}", important: false);
        }


        [Command("createvoice"), Priority(1)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Name for the channel.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null,
                                           [Description("Parent category.")] DiscordChannel parent = null)
            => this.CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);

        [Command("createvoice"), Priority(0)]
        public Task CreateVoiceChannelAsync(CommandContext ctx,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [Description("Name for the channel.")] string name,
                                           [Description("User limit.")] int? userlimit = null,
                                           [Description("Bitrate.")] int? bitrate = null)
            => this.CreateVoiceChannelAsync(ctx, name, parent, userlimit, bitrate);
        #endregion

        #region COMMAND_CHANNEL_DELETE
        [Command("delete"), Priority(1), UsesInteractivity]
        [Description("Delete a given channel or category. If the channel isn't given, deletes the current one. " +
                     "You can also specify reason for deletion.")]
        [Aliases("-", "del", "d", "remove", "rm")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel to delete.")] DiscordChannel channel = null,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            channel = channel ?? ctx.Channel;

            if (channel.Type == ChannelType.Category && channel.Children.Any()) {
                if (await ctx.WaitForBoolReplyAsync("The channel specified is a non-empty category and deleting it will delete child channels as well. Continue?")) {
                    foreach (DiscordChannel child in channel.Children.ToList())
                        await child.DeleteAsync(reason: ctx.BuildInvocationDetailsString(reason));
                }
            } else {
                if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to delete channel {Formatter.Bold(channel.Name)} (ID: {Formatter.InlineCode(channel.Id.ToString())})? This cannot be undone!"))
                    return;
            }

            string name = Formatter.Bold(channel.Name);
            await channel.DeleteAsync(reason: ctx.BuildInvocationDetailsString(reason));
            if (channel.Id != ctx.Channel.Id)
                await this.InformAsync(ctx, $"Successfully deleted channel: {name}", important: false);
        }

        [Command("delete"), Priority(0)]
        public Task DeleteAsync(CommandContext ctx,
                               [RemainingText, Description("Reason.")] string reason)
            => this.DeleteAsync(ctx, null, reason);
        #endregion

        #region COMMAND_CHANNEL_INFO
        [Command("info")]
        [Description("Print information about a given channel. If the channel is not given, uses the current one.")]
        [Aliases("i", "information")]

        public Task InfoAsync(CommandContext ctx,
                             [Description("Channel.")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            if (ctx.Channel.IsPrivate || !ctx.Member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                throw new CommandFailedException("You are not allowed to see this channel! (You thought you are smart, right?)");

            var emb = new DiscordEmbedBuilder {
                Title = channel.ToString(),
                Description = $"Current channel topic: {Formatter.Italic(string.IsNullOrWhiteSpace(channel.Topic) ? "None" : channel.Topic)}",
                Color = this.ModuleColor
            };

            emb.AddField("Type", channel.Type.ToString(), inline: true);
            emb.AddField("NSFW", channel.IsNSFW.ToString(), inline: true);
            emb.AddField("Private", channel.IsPrivate.ToString(), inline: true);
            emb.AddField("Position", channel.Position.ToString(), inline: true);
            if (channel.PerUserRateLimit.HasValue)
                emb.AddField("Per-user rate limit", channel.PerUserRateLimit.ToString(), inline: true);
            if (channel.Type == ChannelType.Voice) {
                emb.AddField("Bitrate", channel.Bitrate.ToString(), inline: true);
                emb.AddField("User limit", channel.UserLimit == 0 ? "No limit." : channel.UserLimit.ToString(), inline: true);
            }
            emb.AddField("Creation time", channel.CreationTimestamp.ToString(), inline: true);

            return ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_CHANNEL_MODIFY
        [Command("modify"), Priority(1)]
        [Description("Modify a given voice channel. Give 0 as an argument if you wish to keep the value unchanged.")]
        [Aliases("edit", "mod", "m", "e")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Voice channel to edit")] DiscordChannel channel,
                                     [Description("User limit.")] int limit = 0,
                                     [Description("Bitrate.")] int bitrate = 0,
                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidCommandUsageException("You can only modify voice channels!");

            if (limit == 0 && bitrate == 0)
                throw new InvalidCommandUsageException("You need to specify atleast one change for either bitrate or user limit.");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                if (limit > 0)
                    m.Userlimit = limit;
                if (bitrate > 0)
                    m.Bitrate = bitrate;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            })).ConfigureAwait(false);

            await this.InformAsync(ctx, $"Successfully modified voice channel {Formatter.Bold(channel.Name)}:\n\nUser limit: {(limit > 0 ? limit.ToString() : "Not changed")}\nBitrate: {(bitrate > 0 ? bitrate.ToString() : "Not changed")}", important: false);
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("User limit.")] int limit = 0,
                               [Description("Bitrate.")] int bitrate = 0,
                               [RemainingText, Description("Reason.")] string reason = null)
            => this.ModifyAsync(ctx, null, limit, bitrate, reason);
        #endregion

        #region COMMAND_CHANNEL_RENAME
        [Command("rename"), Priority(2)]
        [Description("Rename given channel. If the channel is not given, renames the current one.")]
        [Aliases("r", "name", "setname", "rn")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task RenameAsync(CommandContext ctx,
                                     [Description("Reason.")] string reason,
                                     [Description("Channel to rename.")] DiscordChannel channel,
                                     [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new channel name.");

            if (newname.Length < 2 || newname.Length > 100)
                throw new InvalidCommandUsageException("Channel name must be longer than 2 and shorter than 100 characters.");

            channel = channel ?? ctx.Channel;

            if (channel.Type == ChannelType.Text && newname.Contains(' '))
                throw new InvalidCommandUsageException("Text channel name cannot contain spaces.");

            string name = channel.Name;
            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully renamed channel {Formatter.Bold(name)} to {Formatter.Bold(channel.Name)}", important: false);
        }

        [Command("rename"), Priority(1)]
        public Task RenameAsync(CommandContext ctx,
                               [Description("Channel to rename.")] DiscordChannel channel,
                               [RemainingText, Description("New name.")] string newname)
            => this.RenameAsync(ctx, null, channel, newname);

        [Command("rename"), Priority(0)]
        public Task RenameAsync(CommandContext ctx,
                               [RemainingText, Description("New name.")] string newname)
            => this.RenameAsync(ctx, null, null, newname);
        #endregion

        #region COMMAND_CHANNEL_SETNSFW
        [Command("setnsfw"), Priority(2)]
        [Description("Set whether this channel is NSFW or not. You can also provide a reason for the change.")]
        [Aliases("nsfw")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChangeNsfwAsync(CommandContext ctx,
                                         [Description("Set NSFW?")] bool nsfw,
                                         [Description("Channel.")] DiscordChannel channel = null,
                                         [RemainingText, Description("Reason.")] string reason = null)
        {
            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Only text channels can be flagged as NSFW.");

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Nsfw = nsfw;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully set the NSFW var of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(nsfw.ToString())}", important: false);
        }

        [Command("setnsfw"), Priority(1)]
        public Task ChangeNsfwAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Set NSFW?")] bool nsfw,
                                   [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeNsfwAsync(ctx, nsfw, channel, reason);

        [Command("setnsfw"), Priority(0)]
        public Task ChangeNsfwAsync(CommandContext ctx,
                                     [Description("Channel.")] DiscordChannel channel = null,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeNsfwAsync(ctx, true, channel, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPARENT
        [Command("setparent"), Priority(1)]
        [Description("Change the given channel's parent. If the channel is not given, uses the current one. " +
                     "You can also provide a reason.")]
        [Aliases("setpar", "par", "parent")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ChangeParentAsync(CommandContext ctx,
                                           [Description("Child channel.")] DiscordChannel channel,
                                           [Description("Parent category.")] DiscordChannel parent,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (parent.Type != ChannelType.Category)
                throw new CommandFailedException("Parent must be a category.");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Parent = parent;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully set the parent of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(parent.Name)}", important: false);
        }

        [Command("setparent"), Priority(0)]
        public Task ChangeParentAsync(CommandContext ctx,
                                     [Description("Parent category.")] DiscordChannel parent,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.ChangeParentAsync(ctx, null, parent, reason);
        #endregion

        #region COMMAND_CHANNEL_SETPOSITION
        [Command("setposition"), Priority(2)]
        [Description("Change the position of the given channel in the guild channel list. If the channel " +
                     "is not given, repositions the current one. You can also provide reason.")]
        [Aliases("setpos", "pos", "position")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task ReorderChannelAsync(CommandContext ctx,
                                             [Description("Channel to reposition.")] DiscordChannel channel,
                                             [Description("New position.")] int position,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (position < 0)
                throw new ArgumentException("Position cannot be negative.", nameof(position));

            channel = channel ?? ctx.Channel;

            await channel.ModifyPositionAsync(position, ctx.BuildInvocationDetailsString(reason));
            await this.InformAsync(ctx, $"Changed the position of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(position.ToString())}", important: false);
        }

        [Command("setposition"), Priority(1)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [Description("Channel to reorder.")] DiscordChannel channel,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => this.ReorderChannelAsync(ctx, channel, position, reason);

        [Command("setposition"), Priority(0)]
        public Task ReorderChannelAsync(CommandContext ctx,
                                       [Description("Position.")] int position,
                                       [RemainingText, Description("Reason.")] string reason = null)
            => this.ReorderChannelAsync(ctx, null, position, reason);
        #endregion

        #region COMMAND_CHANNEL_SETRATELIMIT
        [Command("setratelimit"), Priority(1)]
        [Description("Set the per-user ratelimit for given channel. Setting the value to 0 will disable ratelimit.")]
        [Aliases("setrl", "setrate", "setrlimit")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetRatelimitAsync(CommandContext ctx,
                                           [Description("Channel to affect.")] DiscordChannel channel,
                                           [Description("New ratelimit.")] int ratelimit,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (ratelimit < 0)
                throw new InvalidCommandUsageException("Ratelimit value cannot be negative.");

            //if (!_ratelimitValues.Contains(ratelimit))
            //    throw new InvalidCommandUsageException($"Ratelimit value must be one of the following: {Formatter.InlineCode(string.Join(", ", _ratelimitValues))}");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.PerUserRateLimit = ratelimit;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Changed the ratemilit setting of channel {Formatter.Bold(channel.Name)} to {Formatter.Bold(ratelimit.ToString())}", important: false);
        }

        [Command("setratelimit"), Priority(0)]
        public Task SetRatelimitAsync(CommandContext ctx,
                                     [Description("New ratelimit.")] int ratelimit,
                                     [Description("Channel to affect.")] DiscordChannel channel,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.SetRatelimitAsync(ctx, channel, ratelimit, reason);

        [Command("setratelimit"), Priority(0)]
        public Task SetRatelimitAsync(CommandContext ctx,
                                     [Description("New ratelimit.")] int ratelimit,
                                     [RemainingText, Description("Reason.")] string reason = null)
            => this.SetRatelimitAsync(ctx, ctx.Channel, ratelimit, reason);
        #endregion

        #region COMMAND_CHANNEL_SETTOPIC
        [Command("settopic"), Priority(2)]
        [Description("Set channel topic. If the channel is not given, uses the current one.")]
        [Aliases("t", "topic", "sett")]

        [RequirePermissions(Permissions.ManageChannels)]
        public async Task SetChannelTopicAsync(CommandContext ctx,
                                              [Description("Reason.")] string reason,
                                              [Description("Channel.")] DiscordChannel channel,
                                              [RemainingText, Description("New topic.")] string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new InvalidCommandUsageException("Missing topic.");

            if (topic.Length > 1024)
                throw new InvalidCommandUsageException("Topic cannot exceed 1024 characters!");

            channel = channel ?? ctx.Channel;

            await channel.ModifyAsync(new Action<ChannelEditModel>(m => {
                m.Topic = topic;
                m.AuditLogReason = ctx.BuildInvocationDetailsString(reason);
            }));

            await this.InformAsync(ctx, $"Successfully changed the topic for channel {Formatter.Bold(channel.Name)}", important: false);
        }

        [Command("settopic"), Priority(1)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [Description("Channel.")] DiscordChannel channel,
                                        [RemainingText, Description("New Topic.")] string topic)
            => this.SetChannelTopicAsync(ctx, null, channel, topic);

        [Command("settopic"), Priority(0)]
        public Task SetChannelTopicAsync(CommandContext ctx,
                                        [RemainingText, Description("New Topic.")] string topic)
            => this.SetChannelTopicAsync(ctx, null, null, topic);
        #endregion

        #region COMMAND_CHANNEL_VIEWPERMS
        [Command("viewperms"), Priority(3)]
        [Description("View permissions for a member or role in the given channel. If the member is not " +
                     "given, lists the sender's permissions. If the channel is not given, uses the current one.")]
        [Aliases("tp", "perms", "permsfor", "testperms", "listperms")]

        [RequireBotPermissions(Permissions.Administrator)]
        public Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Member.")] DiscordMember member = null,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            member = member ?? ctx.Member;
            channel = channel ?? ctx.Channel;

            string perms = $"{Formatter.Bold(member.DisplayName)} cannot access channel {Formatter.Bold(channel.Name)}.";
            if (member.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                perms = member.PermissionsIn(channel).ToPermissionString();

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = $"Permissions for {member.ToString()} in {channel.ToString()}:",
                Description = perms,
                Color = this.ModuleColor
            }.Build());
        }

        [Command("viewperms"), Priority(2)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Member.")] DiscordMember member = null)
            => this.PrintPermsAsync(ctx, member, channel);

        [Command("viewperms"), Priority(1)]
        public async Task PrintPermsAsync(CommandContext ctx,
                                         [Description("Role.")] DiscordRole role,
                                         [Description("Channel.")] DiscordChannel channel = null)
        {
            if (role.Position > ctx.Member.Hierarchy)
                throw new CommandFailedException("You cannot view permissions for roles which have position above your highest role position.");

            channel = channel ?? ctx.Channel;

            DiscordOverwrite overwrite = null;
            foreach (DiscordOverwrite o in channel.PermissionOverwrites.Where(o => o.Type == OverwriteType.Role)) {
                DiscordRole r = await o.GetRoleAsync();
                if (r.Id == role.Id) {
                    overwrite = o;
                    break;
                }
            }

            var emb = new DiscordEmbedBuilder {
                Title = $"Permissions for {role.ToString()} in {channel.ToString()}:",
                Color = this.ModuleColor
            };

            if (!(overwrite is null)) {
                emb.AddField("Allowed", overwrite.Allowed.ToPermissionString())
                   .AddField("Denied", overwrite.Denied.ToPermissionString());
            } else {
                emb.AddField("No overwrites found, listing default role permissions:\n", role.Permissions.ToPermissionString());
            }

            await ctx.RespondAsync(embed: emb.Build());
        }

        [Command("viewperms"), Priority(0)]
        public Task PrintPermsAsync(CommandContext ctx,
                                   [Description("Channel.")] DiscordChannel channel,
                                   [Description("Role.")] DiscordRole role)
            => this.PrintPermsAsync(ctx, role, channel);
        #endregion


        private async Task CheckPotentialChannelNameAsync(CommandContext ctx, string? name, bool throwIfNull = true, bool textChannel = false)
        {
            if (!string.IsNullOrWhiteSpace(name)) {
                if (name.Length > DiscordLimits.ChannelNameLimit)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-chn-name", DiscordLimits.ChannelNameLimit);

                if (textChannel && name.Any(char.IsWhiteSpace))
                    throw new CommandFailedException(ctx, "cmd-err-chn-name-space");

                if (ctx.Guild.Channels.Select(kvp => kvp.Value.Name.ToLowerInvariant()).Contains(name.ToLowerInvariant())) {
                    if (!await ctx.WaitForBoolReplyAsync("q-chn-exists"))
                        return;
                }
            } else if (throwIfNull) {
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");
            }
        }
    }
}
