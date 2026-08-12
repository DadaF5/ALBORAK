using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("BugReports", Schema = "dbo")]
public class BugReport
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string ReportNumber { get; set; } = string.Empty; // BUG-2026-0001

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty; // Steps to reproduce

    public string? ExpectedBehavior { get; set; }
    public string? ActualBehavior { get; set; }

    [Required, StringLength(100)]
    public string ScreenOrModule { get; set; } = string.Empty; // human-friendly label, secondary to auto-captured fields

    public BugSeverity Severity { get; set; }
    public BugStatus Status { get; set; } = BugStatus.NEW;

    // Reporter info
    [Required]
    public string ReportedByUserId { get; set; } = string.Empty;
    [ForeignKey("ReportedByUserId")]
    public virtual ApplicationUser? ReportedBy { get; set; }

    public DateTime ReportedAt { get; set; } = DateTime.Now;

    // Optional: screenshot path
    public string? ScreenshotPath { get; set; }

    // Optional: link to a future Serilog entry
    public string? RelatedTraceId { get; set; }

    // Triage / resolution
    public string? AdminNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    [ForeignKey("ResolvedByUserId")]
    public virtual ApplicationUser? ResolvedBy { get; set; }

    public bool IsActive { get; set; } = true;
    public byte SortOrder { get; set; }

    // ---- Auto-captured technical context ----
    [StringLength(45)]
    public string? UserIpAddress { get; set; }   // IPv4/IPv6

    [StringLength(200)]
    public string? PageUrl { get; set; }          // e.g. /WorkOrder/Create/12

    [StringLength(100)]
    public string? ControllerName { get; set; }

    [StringLength(100)]
    public string? ActionName { get; set; }

    [StringLength(50)]
    public string? HttpMethod { get; set; }       // GET / POST

    [StringLength(300)]
    public string? UserAgent { get; set; }
}

public enum BugSeverity
{
    LOW = 1,
    MEDIUM = 2,
    HIGH = 3,
    CRITICAL = 4
}

public enum BugStatus
{
    NEW = 1,
    CONFIRMED = 2,
    IN_PROGRESS = 3,
    FIXED = 4,
    WONT_FIX = 5,
    DUPLICATE = 6
}