﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuokkaPack.Data;

#nullable disable

namespace QuokkaPack.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250701224228_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.6");

            modelBuilder.Entity("Data.Models.Trip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Destination")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Trips");
                });
#pragma warning restore 612, 618
        }
    }
}
