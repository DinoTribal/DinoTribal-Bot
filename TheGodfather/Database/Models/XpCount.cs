﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("xp_count")]
    public class XpCount : IEquatable<XpCount>
    {
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("xp")]
        public int Xp { get; set; }


        public bool Equals(XpCount? other)
            => other is { } && this.UserId == other.UserId;

        public override bool Equals(object? other)
            => this.Equals(other as XpCount);

        public override int GetHashCode()
            => this.UserId.GetHashCode();
    }
}
