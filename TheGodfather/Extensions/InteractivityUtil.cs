﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Extensions.Converters;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Extensions
{
    public static class InteractivityUtil
    {
        public static async Task<bool> WaitForConfirmationAsync(CommandContext ctx)
        {
            bool response = false;
            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                m => {
                    if (m.ChannelId != ctx.Channel.Id || m.Author.Id != ctx.User.Id)
                        return false;
                    bool? b = CustomBoolConverter.TryConvert(m.Content);
                    response = b ?? false;
                    return b.HasValue;
                }
            ).ConfigureAwait(false);

            return response;
        }

        public static async Task<DiscordUser> WaitForGameOpponentAsync(CommandContext ctx)
        {
            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Author.Id == ctx.User.Id || xm.Channel.Id != ctx.Channel.Id)
                        return false;
                    var split = xm.Content.ToLowerInvariant().Split(' ');
                    return split.Length == 1 && (split[0] == "me" || split[0] == "i");
                }
            ).ConfigureAwait(false);

            return mctx?.User;
        }

        public static async Task SendPaginatedCollectionAsync<T>(CommandContext ctx,
                                                                 string title,
                                                                 IEnumerable<T> collection, 
                                                                 Func<T, string> formatter,
                                                                 DiscordColor? color = null,
                                                                 int amount = 10,
                                                                 TimeSpan? timeout = null)
        {
            var list = collection.ToList();
            var pages = new List<Page>();
            int pagesnum = (list.Count - 1) / amount;
            for (int i = 0; i <= pagesnum; i++) {
                int start = amount * i;
                int count = start + amount > list.Count ? list.Count - start : amount;
                pages.Add(new Page() {
                    Embed = new DiscordEmbedBuilder() {
                        Title = $"{title} (page {i + 1}/{pagesnum + 1})",
                        Description = string.Join("\n", list.GetRange(start, count).Select(formatter)),
                        Color = color ?? DiscordColor.Black
                    }.Build()
                });
            }
            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, timeout)
                .ConfigureAwait(false);
        }

        public static async Task<DiscordDmChannel> CreateDmChannelAsync(DiscordClient client, ulong uid)
        {
            var firstResult = client.Guilds.Values.SelectMany(e => e.Members).FirstOrDefault(e => e.Id == uid);
            if (firstResult != null)
                return await firstResult.CreateDmChannelAsync().ConfigureAwait(false);
            else
                return null;
        }

        public static async Task<List<string>> WaitAndParsePollOptionsAsync(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var mctx = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (mctx == null)
                return null;

            return mctx.Message.Content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        }
    }
}
