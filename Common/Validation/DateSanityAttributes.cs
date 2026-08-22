using System.ComponentModel.DataAnnotations;

namespace FRAProject.Common.Validation
{
    /// <summary>
    /// APP-WIDE — server-side backstop for any date input field, in any
    /// module. Added after a live bug report against the Component module:
    /// native browser type="date" inputs don't reliably cap a typed year at
    /// 4 digits (a user ended up with a "date de fabrication" of
    /// 02-Feb-20269), and confirmed the same widget/pattern is used across
    /// other modules too — hence this living under Common/, not under any
    /// one Area, so every module's DTOs can reference the same two
    /// attributes instead of each carrying its own copy.
    ///
    /// HTML5 min/max on the &lt;input&gt; itself is a UX nicety, not a
    /// trustworthy data-integrity boundary — a bad value can still arrive by
    /// direct POST regardless of what the browser's widget allowed through.
    /// This attribute is what actually rejects it; pair it with a
    /// min="yyyy-MM-dd" max="yyyy-MM-dd" attribute on the &lt;input&gt; for
    /// immediate client-side feedback (see any Components/*.cshtml view for
    /// the pattern — compute "todayIso"/"minIso" in the view's @{ } block).
    ///
    /// Works on DateOnly, DateOnly?, DateTime, and DateTime? — covers both
    /// conventions this app uses across different modules.
    /// </summary>
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public NotFutureDateAttribute()
        {
            ErrorMessage = "La date ne peut pas être dans le futur.";
        }

        public override bool IsValid(object? value)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return value switch
            {
                null => true, // [Required] (where present) handles the null case separately
                DateOnly d => d <= today,
                DateTime dt => DateOnly.FromDateTime(dt) <= today,
                _ => true // wrong type on this field isn't this attribute's job to flag
            };
        }
    }

    /// <summary>
    /// APP-WIDE — companion floor guard, same root bug as
    /// NotFutureDateAttribute: an extra/misplaced typed digit in the year
    /// segment can just as easily produce an implausibly EARLY date as a
    /// future one. Pick a floor per field via the constructor — e.g. 1940
    /// comfortably predates every aircraft type/component in this fleet for
    /// a manufacture date, but a "date recorded" field elsewhere in the app
    /// might reasonably use a much later, tighter floor (e.g. this system's
    /// own go-live date).
    /// </summary>
    public class NotBeforeAttribute : ValidationAttribute
    {
        private readonly DateOnly _floor;

        public NotBeforeAttribute(int year, int month, int day)
        {
            _floor = new DateOnly(year, month, day);
            ErrorMessage = $"La date ne peut pas être antérieure au {_floor:dd/MM/yyyy}.";
        }

        public override bool IsValid(object? value) => value switch
        {
            null => true,
            DateOnly d => d >= _floor,
            DateTime dt => DateOnly.FromDateTime(dt) >= _floor,
            _ => true
        };
    }
}
