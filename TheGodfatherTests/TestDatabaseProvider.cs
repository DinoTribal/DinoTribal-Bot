﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests
{
    public static class TestDatabaseProvider
    {
        public static DbContextBuilder Database { get; private set; }
        public static string ConnectionString { get; private set; }
        public static SqliteConnection DatabaseConnection { get; private set; }


        static TestDatabaseProvider()
        {
            ConnectionString = "DataSource=:memory:;foreign keys=true;";
            DatabaseConnection = new SqliteConnection(ConnectionString);

            var cfg = new DbConfig() {
                Provider = DbProvider.SqliteInMemory
            };
            DbContextOptions<TheGodfatherDbContext> options = new DbContextOptionsBuilder<TheGodfatherDbContext>()
                .UseSqlite(DatabaseConnection)
                .UseLazyLoadingProxies()
                .Options;

            Database = new DbContextBuilder(cfg, options);
        }


        public static void AlterAndVerify(Action<TheGodfatherDbContext> alter, Action<TheGodfatherDbContext> verify, bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static async Task AlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> alter, Func<TheGodfatherDbContext, Task> verify, bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    await alter(context);
                    if (ensureSave)
                        await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateContext())
                    await verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static void SetupAlterAndVerify(Action<TheGodfatherDbContext> setup,
                                               Action<TheGodfatherDbContext> alter,
                                               Action<TheGodfatherDbContext> verify,
                                               bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    setup(context);
                    context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static async Task SetupAlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> setup,
                                                          Func<TheGodfatherDbContext, Task> alter,
                                                          Func<TheGodfatherDbContext, Task> verify,
                                                          bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    await setup(context);
                    await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateContext()) {
                    await alter(context);
                    if (ensureSave)
                        await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateContext())
                    await verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }


        private static void CreateDatabase()
        {
            using (TheGodfatherDbContext context = Database.CreateContext())
                context.Database.EnsureCreated();
        }

        private static void SeedGuildData()
        {
            using (TheGodfatherDbContext context = Database.CreateContext()) {
                context.Configs.AddRange(MockData.Ids.Select(id => new GuildConfig() { GuildId = id }));
                context.SaveChanges();
            }
        }
    }
}
