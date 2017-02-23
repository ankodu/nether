﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Nether.Data.Sql.Leaderboard;

namespace Nether.Data.Sql.Leaderboard.Migrations
{
    [DbContext(typeof(SqlLeaderboardContext))]
    partial class SqlLeaderboardContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Nether.Data.Sql.Leaderboard.QueriedGamerScore", b =>
                {
                    b.Property<string>("Gamertag")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomTag");

                    b.Property<long>("Ranking");

                    b.Property<int>("Score");

                    b.HasKey("Gamertag");

                    b.ToTable("Ranks");
                });

            modelBuilder.Entity("Nether.Data.Sql.Leaderboard.SavedGamerScore", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomTag")
                        .HasMaxLength(50);

                    b.Property<DateTime>("DateAchieved");

                    b.Property<string>("Gamertag")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("Score");

                    b.HasKey("Id");

                    b.HasIndex("DateAchieved", "Gamertag", "Score");

                    b.ToTable("Scores");

                    b.HasAnnotation("SqlServer:TableName", "Scores");
                });
        }
    }
}
