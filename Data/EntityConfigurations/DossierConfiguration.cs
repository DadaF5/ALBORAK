using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for the ImmatriculationDossier
    /// family of tables:
    ///
    ///   ImmatriculationDossier   ← master
    ///   DossierAuthority         ← Step 1 — shared PK 1:1
    ///   DossierAircraft          ← Step 2 — shared PK 1:1
    ///   DossierAirworthiness     ← Step 3 — shared PK 1:1
    ///   ImmatriculationDocument  ← Step 4 — standard 1:many
    ///
    /// SHARED PK PATTERN (1:1):
    ///   The child table uses DossierId as BOTH its PK and its FK.
    ///   This guarantees strict 1:1 at DB level — no orphan rows,
    ///   no two authority rows for the same dossier.
    ///
    ///   EF Fluent API:
    ///     HasOne(master => master.Child)
    ///       .WithOne(child => child.Dossier)
    ///       .HasForeignKey<ChildType>(child => child.DossierId)
    ///
    ///   The child DossierId is marked [Key] in the model.
    ///   EF infers it is also the FK from the HasForeignKey call.
    ///
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new ImmatriculationDossierConfiguration());
    /// </summary>
    public class ImmatriculationDossierConfiguration : IEntityTypeConfiguration<ImmatriculationDossier>
    {
        public void Configure(EntityTypeBuilder<ImmatriculationDossier> builder)
        {
            // ════════════════════════════════════════════════════════
            //  MASTER TABLE — ImmatriculationDossier
            // ════════════════════════════════════════════════════════

            builder.ToTable("ImmatriculationDossier");
            builder.HasKey(d => d.Id);

            // ── DossierNumber ────────────────────────────────────────
            // Unique when not null — NULL while Brouillon.
            // SQL Server supports unique indexes with multiple NULLs
            // via a filtered index. EF handles this with IsUnique()
            // on a nullable column.
            builder.Property(d => d.DossierNumber)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.HasIndex(d => d.DossierNumber)
                .IsUnique()
                .HasFilter("[DossierNumber] IS NOT NULL")
                .HasDatabaseName("UX_ImmatriculationDossier_Number");

            // ── Status ───────────────────────────────────────────────
            builder.Property(d => d.Status)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Brouillon");

            // ── CurrentStep ──────────────────────────────────────────
            builder.Property(d => d.CurrentStep)
                .HasDefaultValue(1);

            // ── Step 5 attestation fields ────────────────────────────
            builder.Property(d => d.AttestationCity)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(d => d.SignatoryName)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(d => d.AttestationConfirmed)
                .HasDefaultValue(false);

            // ── Audit ────────────────────────────────────────────────
            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(d => d.CreatedByUserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            // ════════════════════════════════════════════════════════
            //  1:1 — DossierAuthority (Step 1)
            //  Shared PK: DossierAuthority.DossierId = ImmatriculationDossier.Id
            // ════════════════════════════════════════════════════════
            builder.HasOne(d => d.Authority)
                .WithOne(a => a.Dossier)
                .HasForeignKey<DossierAuthority>(a => a.DossierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DossierAuthority_Dossier");

            // ════════════════════════════════════════════════════════
            //  1:1 — DossierAircraft (Step 2)
            // ════════════════════════════════════════════════════════
            builder.HasOne(d => d.Aircraft)
                .WithOne(a => a.Dossier)
                .HasForeignKey<DossierAircraft>(a => a.DossierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DossierAircraft_Dossier");

            // ════════════════════════════════════════════════════════
            //  1:1 — DossierAirworthiness (Step 3)
            // ════════════════════════════════════════════════════════
            builder.HasOne(d => d.Airworthiness)
                .WithOne(a => a.Dossier)
                .HasForeignKey<DossierAirworthiness>(a => a.DossierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DossierAirworthiness_Dossier");

            // ════════════════════════════════════════════════════════
            //  1:many — ImmatriculationDocument (Step 4)
            //  Standard FK pattern — document has its own PK
            // ════════════════════════════════════════════════════════
            builder.HasMany(d => d.Documents)
                .WithOne(doc => doc.Dossier)
                .HasForeignKey(doc => doc.DossierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ImmatriculationDocument_Dossier");
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 1 — DossierAuthority configuration
    // ════════════════════════════════════════════════════════════════
    public class ImmatriculationDossierAuthorityConfiguration
        : IEntityTypeConfiguration<DossierAuthority>
    {
        public void Configure(EntityTypeBuilder<DossierAuthority> builder)
        {
            builder.ToTable("DossierAuthority");

            // Shared PK — DossierId is both PK and FK
            // The FK relationship is declared on the master (above).
            // Here we only declare column properties.
            builder.HasKey(a => a.DossierId);

            // ── FK → EmployingAuthority ──────────────────────────────
            builder.HasOne(a => a.EmployingAuthority)
                .WithMany()
                .HasForeignKey(a => a.EmployingAuthorityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAuthority_EmployingAuthority");

            // ── FK → Base (BaseAerienneId) ───────────────────────────
            // Single FK to Base — no dual FK complexity here
            builder.HasOne(a => a.BaseAerienne)
                .WithMany()
                .HasForeignKey(a => a.BaseAerienneId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAuthority_Base");

            // ── Column properties ────────────────────────────────────
            builder.Property(a => a.OgmnNumber)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(a => a.OgmnSousPartie)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired(false);

            builder.Property(a => a.OgmnResponsable)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(a => a.AeAddress)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(a => a.AePhone)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(a => a.AeEmail)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 2 — DossierAircraft configuration
    // ════════════════════════════════════════════════════════════════
    public class ImmatriculationDossierAircraftConfiguration
        : IEntityTypeConfiguration<DossierAircraft>
    {
        public void Configure(EntityTypeBuilder<DossierAircraft> builder)
        {
            builder.ToTable("DossierAircraft");
            builder.HasKey(a => a.DossierId);

            // ── FK → AcCategory ──────────────────────────────────────
            builder.HasOne(a => a.AircraftCategory)
                .WithMany()
                .HasForeignKey(a => a.AircraftCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_AcCategory");

            // ── FK → AcType ──────────────────────────────────────────
            builder.HasOne(a => a.AcType)
                .WithMany()
                .HasForeignKey(a => a.AcTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_AcType");

            // ── FK → AircraftVersion ─────────────────────────────────
            builder.HasOne(a => a.AircraftVersion)
                .WithMany()
                .HasForeignKey(a => a.AircraftVersionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_AircraftVersion");

            // ── FK → MissionRole ─────────────────────────────────────
            builder.HasOne(a => a.MissionRole)
                .WithMany()
                .HasForeignKey(a => a.MissionRoleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_MissionRole");

            // ── FK → AircraftManufacturer ────────────────────────────
            builder.HasOne(a => a.Manufacturer)
                .WithMany()
                .HasForeignKey(a => a.ManufacturerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_Manufacturer");

            // ── FK → Base (PortAttacheId) ────────────────────────────
            // Single FK — no dual FK in this model
            builder.HasOne(a => a.PortAttache)
                .WithMany()
                .HasForeignKey(a => a.PortAttacheId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_PortAttache");

            // ── FK → Country (OriginCountryId) ───────────────────────
            // Single FK — ForeignCountryId is on DossierAirworthiness
            builder.HasOne(a => a.OriginCountry)
                .WithMany()
                .HasForeignKey(a => a.OriginCountryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAircraft_OriginCountry");

            // ── ImmatriculationSuffix — unique when not null ──────────
            builder.Property(a => a.ImmatriculationSuffix)
                .HasColumnType("nvarchar(5)")
                .HasMaxLength(5)
                .IsRequired(false);

            builder.HasIndex(a => a.ImmatriculationSuffix)
                .IsUnique()
                .HasFilter("[ImmatriculationSuffix] IS NOT NULL")
                .HasDatabaseName("UX_DossierAircraft_ImmatriculationSuffix");

            // ── Other string columns ─────────────────────────────────
            builder.Property(a => a.AircraftSerie)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(a => a.SerialNumber)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 3 — DossierAirworthiness configuration
    // ════════════════════════════════════════════════════════════════
    public class ImmatriculationDossierAirworthinessConfiguration
        : IEntityTypeConfiguration<DossierAirworthiness>
    {
        public void Configure(EntityTypeBuilder<DossierAirworthiness> builder)
        {
            builder.ToTable("DossierAirworthiness");
            builder.HasKey(a => a.DossierId);

            // ── FK → CdnDocType ──────────────────────────────────────
            builder.HasOne(a => a.CdnDocType)
                .WithMany()
                .HasForeignKey(a => a.CdnDocTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAirworthiness_CdnDocType");

            // ── FK → Country (ForeignCountryId) ─────────────────────
            // Single FK to Country — OriginCountryId is on DossierAircraft.
            // No dual FK in either model. Clean.
            builder.HasOne(a => a.ForeignCountry)
                .WithMany()
                .HasForeignKey(a => a.ForeignCountryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DossierAirworthiness_ForeignCountry");

            // ── Column defaults ──────────────────────────────────────
            builder.Property(a => a.HasAirworthinessDoc)
                .HasDefaultValue(false);

            builder.Property(a => a.CdnRenewalRequested)
                .HasDefaultValue(false);

            builder.Property(a => a.WasForeignRegistered)
                .HasDefaultValue(false);

            // ── String columns ───────────────────────────────────────
            builder.Property(a => a.CdnReference)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(a => a.FormerImmatriculation)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 4 — ImmatriculationDocument configuration
    // ════════════════════════════════════════════════════════════════
    public class ImmatriculationDossierDocumentConfiguration
        : IEntityTypeConfiguration<ImmatriculationDocument>
    {
        public void Configure(EntityTypeBuilder<ImmatriculationDocument> builder)
        {
            builder.ToTable("ImmatriculationDocument");
            builder.HasKey(d => d.Id);

            // FK to Dossier declared on master (Cascade delete)

            // ── FK → ImmatriculationDocType ──────────────────────────
            builder.HasOne(d => d.DocumentType)
                .WithMany()
                .HasForeignKey(d => d.DocumentTypeId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ImmatriculationDocument_DocType");

            // ── Unique: one active upload per document type per dossier
            // Allows replacing (soft delete old, insert new)
            // but prevents two active uploads for the same type.
            builder.HasIndex(d => new { d.DossierId, d.DocumentTypeId, d.IsActive })
                .HasDatabaseName("IX_ImmatriculationDocument_Dossier_Type");

            // ── File metadata ────────────────────────────────────────
            builder.Property(d => d.FilePath)
                .HasColumnType("nvarchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(d => d.FileName)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(d => d.MimeType)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(d => d.UploadedByUserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);
        }
    }
}