﻿// <auto-generated />
using System;
using BerichtBotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BerichtBotNet.Migrations
{
    [DbContext(typeof(BerichtBotContext))]
    [Migration("20230625131307_DiscordUserIdIsUnique")]
    partial class DiscordUserIdIsUnique
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BerichtBotNet.Data.Apprentice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DiscordUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("GroupId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SkipCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DiscordUserId")
                        .IsUnique();

                    b.HasIndex("GroupId");

                    b.ToTable("Apprentices");
                });

            modelBuilder.Entity("BerichtBotNet.Data.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DiscordGroupId")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("BerichtBotNet.Data.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ApprenticeId")
                        .HasColumnType("integer");

                    b.Property<int>("BerichtheftNummer")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ApprenticeId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("BerichtBotNet.Data.Apprentice", b =>
                {
                    b.HasOne("BerichtBotNet.Data.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("BerichtBotNet.Data.Log", b =>
                {
                    b.HasOne("BerichtBotNet.Data.Apprentice", "Apprentice")
                        .WithMany()
                        .HasForeignKey("ApprenticeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Apprentice");
                });
#pragma warning restore 612, 618
        }
    }
}
