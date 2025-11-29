using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace FRAProject.Data
{
    public class FRAContext : DbContext
    {
        public FRAContext(DbContextOptions<FRAContext> options)
            : base(options)
        {
        }

        // =====================================
        // DbSets (Base Department and Person related BbSets)
        // =====================================
        public DbSet<Person> Persons { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<RankType> RankTypes { get; set; }
        public DbSet<Base> Bases { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SubDepartment> SubDepartments { get; set; }


        //===============================
        // Aircraft Related DbSets
        //===============================
        public DbSet<AcCategory> AcCategories { get; set; }
        public DbSet<AcMainGroup> AcMainGroups { get; set; }
        public DbSet<AcType> AcTypes { get; set; }
        public DbSet<AcStatusType> AcStatusTypes { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }

        // Add other DbSets later...



        // =====================================
        // OnModelCreating Override
        // =====================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Prevent duplicates : tailNo + RegistrationNumber + IntCode per AcType
        }

    }
}

