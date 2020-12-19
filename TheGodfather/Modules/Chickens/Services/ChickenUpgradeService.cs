﻿using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens.Services
{
    public sealed class ChickenUpgradeService : DbAbstractionServiceBase<ChickenUpgrade, int>
    {
        public override bool IsDisabled => false;


        public ChickenUpgradeService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<ChickenUpgrade> DbSetSelector(TheGodfatherDbContext db)
            => db.ChickenUpgrades;

        public override ChickenUpgrade EntityFactory(int id) 
            => new ChickenUpgrade { Id = id };
        
        public override int EntityIdSelector(ChickenUpgrade entity) 
            => entity.Id;
        
        public override object[] EntityPrimaryKeySelector(int id) 
            => new object[] { id };
    }
}
