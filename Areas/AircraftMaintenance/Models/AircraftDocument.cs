// Areas/AircraftMaintenance/Models/AircraftDocument.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    [Table("AircraftDocuments", Schema = "dbo")]
    public class AircraftDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = default!;

        [Required]
        public int DocumentTypeId { get; set; }
        public AircraftDocumentType DocumentType { get; set; } = default!;

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        [StringLength(50)]
        public string? Revision { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        public DateTime? IssuedAtUtc { get; set; }
        public DateTime? ValidFromUtc { get; set; }
        public DateTime? ValidUntilUtc { get; set; }

        public bool IsCurrent { get; set; } = false;

        [StringLength(30)]
        public string? Status { get; set; }

        // Optional storage fields
        [StringLength(300)]
        public string? StorageKey { get; set; }

        [StringLength(255)]
        public string? FileName { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
        [StringLength(200)]
        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}