using FRAProject.Enums;

public class MedicalFitnessResult
{
    // ============================
    // Final operational decision
    // ============================

    /// <summary>
    /// Final decision after applying system rules
    /// (EXPIRED always => UNFIT)
    /// </summary>
    public MedicalDecision Decision { get; set; }

    /// <summary>
    /// System-calculated validity based on duration
    /// </summary>
    public MedicalValidity Validity { get; set; }

    // ============================
    // Convenience (SAFE)
    // ============================

    public bool IsExpired => Validity == MedicalValidity.EXPIRED;
    public bool IsFit => Decision == MedicalDecision.FIT && !IsExpired;

    // ============================
    // Time computation
    // ============================

    /// <summary>
    /// Number of days remaining until expiry (0 if expired or no check)
    /// </summary>
    public int RemainingDays { get; set; }

    /// <summary>
    /// Computed expiry date of the medical check
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    // ============================
    // Source (audit / UI)
    // ============================

    /// <summary>
    /// Type of the medical check used for computation
    /// </summary>
    public MedicalCheckType? SourceCheckType { get; set; }

    /// <summary>
    /// ID of the medical check used (optional but useful)
    /// </summary>
    public int? MedicalCheckId { get; set; }
}
