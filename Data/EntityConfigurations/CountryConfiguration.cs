using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for the Country entity.
    /// Kept in a separate IEntityTypeConfiguration class to avoid
    /// bloating FRAContext.OnModelCreating() as the model grows.
    ///
    /// Apply in FRAContext via:
    ///   builder.ApplyConfiguration(new CountryConfiguration());
    /// </summary>
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("Country");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(c => c.Id);

            // ── IsoCode ──────────────────────────────────────────────────
            // CHAR(2) — fixed length, always exactly 2 characters
            // Unique index — no two countries share the same ISO code
            builder.Property(c => c.IsoCode)
                .HasColumnType("char(2)")
                .IsFixedLength()
                .IsRequired()
                .HasMaxLength(2);

            builder.HasIndex(c => c.IsoCode)
                .IsUnique()
                .HasDatabaseName("UX_Country_IsoCode");

            // ── Name ─────────────────────────────────────────────────────
            // Unique index — no two countries share the same name
            builder.Property(c => c.Name)
                .HasColumnType("nvarchar(100)")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("UX_Country_Name");

            // ── Continent ────────────────────────────────────────────────
            builder.Property(c => c.Continent)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(c => c.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // HasData() uses Id values to track rows across migrations.
            // NEVER change an Id once seeded — EF uses it as the key
            // for UPDATE/DELETE in subsequent migrations.
            //
            // Countries are ordered by SortOrder then Name.
            // Morocco (MA) is SortOrder=1 — always first in DDLs.
            // Key partner/supplier countries follow (1–10).
            // Remaining countries are alphabetical (SortOrder=99).
            builder.HasData(SeedData());
        }

        // ── Seed data ────────────────────────────────────────────────────
        private static IEnumerable<Country> SeedData() =>
        [
            // ── Morocco first — the home country ─────────────────────────
            new Country { Id =  1, IsoCode = "MA", Name = "Maroc",
                Continent = "Afrique",          SortOrder = 1,  IsActive = true },

            // ── Key partner / supplier countries (SortOrder 2–10) ────────
            new Country { Id =  2, IsoCode = "US", Name = "Etats-Unis",
                Continent = "Amerique du Nord", SortOrder = 2,  IsActive = true },
            new Country { Id =  3, IsoCode = "FR", Name = "France",
                Continent = "Europe",           SortOrder = 3,  IsActive = true },
            new Country { Id =  4, IsoCode = "GB", Name = "Royaume-Uni",
                Continent = "Europe",           SortOrder = 4,  IsActive = true },
            new Country { Id =  5, IsoCode = "DE", Name = "Allemagne",
                Continent = "Europe",           SortOrder = 5,  IsActive = true },
            new Country { Id =  6, IsoCode = "IT", Name = "Italie",
                Continent = "Europe",           SortOrder = 6,  IsActive = true },
            new Country { Id =  7, IsoCode = "ES", Name = "Espagne",
                Continent = "Europe",           SortOrder = 7,  IsActive = true },
            new Country { Id =  8, IsoCode = "RU", Name = "Russie",
                Continent = "Europe / Asie",    SortOrder = 8,  IsActive = true },
            new Country { Id =  9, IsoCode = "CN", Name = "Chine",
                Continent = "Asie",             SortOrder = 9,  IsActive = true },
            new Country { Id = 10, IsoCode = "BR", Name = "Bresil",
                Continent = "Amerique du Sud",  SortOrder = 10, IsActive = true },

            // ── Africa ───────────────────────────────────────────────────
            new Country { Id = 11, IsoCode = "DZ", Name = "Algerie",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 12, IsoCode = "TN", Name = "Tunisie",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 13, IsoCode = "LY", Name = "Libye",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 14, IsoCode = "EG", Name = "Egypte",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 15, IsoCode = "SN", Name = "Senegal",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 16, IsoCode = "NG", Name = "Nigeria",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },
            new Country { Id = 17, IsoCode = "ZA", Name = "Afrique du Sud",
                Continent = "Afrique",          SortOrder = 99, IsActive = true },

            // ── Middle East ───────────────────────────────────────────────
            new Country { Id = 18, IsoCode = "SA", Name = "Arabie Saoudite",
                Continent = "Moyen-Orient",     SortOrder = 99, IsActive = true },
            new Country { Id = 19, IsoCode = "AE", Name = "Emirats Arabes Unis",
                Continent = "Moyen-Orient",     SortOrder = 99, IsActive = true },
            new Country { Id = 20, IsoCode = "TR", Name = "Turquie",
                Continent = "Europe / Asie",    SortOrder = 99, IsActive = true },
            new Country { Id = 21, IsoCode = "IL", Name = "Israel",
                Continent = "Moyen-Orient",     SortOrder = 99, IsActive = true },
            new Country { Id = 22, IsoCode = "JO", Name = "Jordanie",
                Continent = "Moyen-Orient",     SortOrder = 99, IsActive = true },

            // ── Europe ───────────────────────────────────────────────────
            new Country { Id = 23, IsoCode = "NL", Name = "Pays-Bas",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 24, IsoCode = "BE", Name = "Belgique",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 25, IsoCode = "CH", Name = "Suisse",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 26, IsoCode = "SE", Name = "Suede",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 27, IsoCode = "PT", Name = "Portugal",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 28, IsoCode = "PL", Name = "Pologne",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 29, IsoCode = "CZ", Name = "Republique tcheque",
                Continent = "Europe",           SortOrder = 99, IsActive = true },
            new Country { Id = 30, IsoCode = "UA", Name = "Ukraine",
                Continent = "Europe",           SortOrder = 99, IsActive = true },

            // ── Americas ─────────────────────────────────────────────────
            new Country { Id = 31, IsoCode = "CA", Name = "Canada",
                Continent = "Amerique du Nord", SortOrder = 99, IsActive = true },
            new Country { Id = 32, IsoCode = "MX", Name = "Mexique",
                Continent = "Amerique du Nord", SortOrder = 99, IsActive = true },

            // ── Asia-Pacific ──────────────────────────────────────────────
            new Country { Id = 33, IsoCode = "JP", Name = "Japon",
                Continent = "Asie",             SortOrder = 99, IsActive = true },
            new Country { Id = 34, IsoCode = "KR", Name = "Coree du Sud",
                Continent = "Asie",             SortOrder = 99, IsActive = true },
            new Country { Id = 35, IsoCode = "IN", Name = "Inde",
                Continent = "Asie",             SortOrder = 99, IsActive = true },
            new Country { Id = 36, IsoCode = "PK", Name = "Pakistan",
                Continent = "Asie",             SortOrder = 99, IsActive = true },
            new Country { Id = 37, IsoCode = "AU", Name = "Australie",
                Continent = "Oceanie",          SortOrder = 99, IsActive = true },
        ];
    }
}
