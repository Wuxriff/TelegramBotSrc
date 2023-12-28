﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReminderBot.DataAccess;

#nullable disable

namespace ReminderBot.DataAccess.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.10");

            modelBuilder.Entity("ReminderBot.Entities.Reminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Base64Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDateUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateSentUtc")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("OriginalDateSentUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("OriginalReminderDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("OriginalReminderDateUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReminderDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReminderDateUtc")
                        .HasColumnType("TEXT");

                    b.Property<int>("TelegramUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TelegramUserId");

                    b.ToTable("Reminders", (string)null);
                });

            modelBuilder.Entity("ReminderBot.Entities.TelegramUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TelegramUsers", (string)null);
                });

            modelBuilder.Entity("ReminderBot.Entities.TelegramUserSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CountryCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("CountryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("DateTimeFormat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DateTimeFormatType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAutoDeleteMessages")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsPaused")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<double?>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<int>("PostponeMinutes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TelegramUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TimeZoneId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TelegramUserId")
                        .IsUnique();

                    b.ToTable("TelegramUserSettings", (string)null);
                });

            modelBuilder.Entity("ReminderBot.Entities.Reminder", b =>
                {
                    b.HasOne("ReminderBot.Entities.TelegramUser", "TelegramUser")
                        .WithMany("Reminders")
                        .HasForeignKey("TelegramUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("TelegramUser");
                });

            modelBuilder.Entity("ReminderBot.Entities.TelegramUserSettings", b =>
                {
                    b.HasOne("ReminderBot.Entities.TelegramUser", "TelegramUser")
                        .WithOne("TelegramUserSettings")
                        .HasForeignKey("ReminderBot.Entities.TelegramUserSettings", "TelegramUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TelegramUser");
                });

            modelBuilder.Entity("ReminderBot.Entities.TelegramUser", b =>
                {
                    b.Navigation("Reminders");

                    b.Navigation("TelegramUserSettings")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
