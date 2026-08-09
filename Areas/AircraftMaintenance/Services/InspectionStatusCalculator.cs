using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    // Shared logic for computing InspectionState.NextDue* and StatusSnapshot.
    // Used by WorkOrder.Close() (sets a fresh snapshot right after
    // completion) and intended for reuse by the future DueList view
    // (recomputes status against CURRENT aircraft position, not just at
    // close time) — same rules, two different "current position" inputs.
    public static class InspectionStatusCalculator
    {
        // Computes NextDueHours/Cycles/Date from a reference point (usually
        // WorkOrder.CloseHours/Cycles/Date) plus the InspectionType's
        // interval settings.
        public static (int? NextDueHours, int? NextDueCycles, DateOnly? NextDueDate) ComputeNextDue(
            int? refHours, int? refCycles, DateOnly? refDate, InspectionType type)
        {
            int? nextHours = (type.IntervalHours.HasValue && refHours.HasValue)
                ? refHours.Value + type.IntervalHours.Value
                : null;

            int? nextCycles = (type.IntervalCycles.HasValue && refCycles.HasValue)
                ? refCycles.Value + type.IntervalCycles.Value
                : null;

            DateOnly? nextDate = (type.CalendarValue.HasValue && refDate.HasValue)
                ? AddCalendarPeriod(refDate.Value, type.CalendarValue.Value, type.CalendarUnit)
                : null;

            return (nextHours, nextCycles, nextDate);
        }

        // Compares a CURRENT aircraft position against NextDue values (with
        // tolerance) to produce OVERDUE | ALERT | OK | UNKNOWN.
        public static string ComputeStatus(
            int? currentHours, int? currentCycles, DateOnly? currentDate,
            int? nextDueHours, int? nextDueCycles, DateOnly? nextDueDate,
            InspectionType type)
        {
            bool anyDueSet = nextDueHours.HasValue || nextDueCycles.HasValue || nextDueDate.HasValue;
            if (!anyDueSet) return "UNKNOWN";

            bool overdue = false;
            bool alert = false;

            if (nextDueHours.HasValue && currentHours.HasValue)
            {
                if (currentHours.Value >= nextDueHours.Value)
                {
                    overdue = true;
                }
                else if (type.ToleranceHours.HasValue &&
                         currentHours.Value >= nextDueHours.Value - type.ToleranceHours.Value)
                {
                    alert = true;
                }
            }

            if (nextDueCycles.HasValue && currentCycles.HasValue)
            {
                if (currentCycles.Value >= nextDueCycles.Value)
                {
                    overdue = true;
                }
                else if (type.ToleranceCycles.HasValue &&
                         currentCycles.Value >= nextDueCycles.Value - type.ToleranceCycles.Value)
                {
                    alert = true;
                }
            }

            if (nextDueDate.HasValue && currentDate.HasValue)
            {
                if (currentDate.Value >= nextDueDate.Value)
                {
                    overdue = true;
                }
                else
                {
                    var toleranceDays = CalendarPeriodToDays(type.ToleranceCalendarValue, type.ToleranceCalendarUnit);
                    if (toleranceDays.HasValue && currentDate.Value >= nextDueDate.Value.AddDays(-toleranceDays.Value))
                    {
                        alert = true;
                    }
                }
            }

            if (overdue) return "OVERDUE";
            if (alert) return "ALERT";
            return "OK";
        }

        private static DateOnly AddCalendarPeriod(DateOnly baseDate, int value, string? unit) => unit switch
        {
            "DAY" => baseDate.AddDays(value),
            "MONTH" => baseDate.AddMonths(value),
            "YEAR" => baseDate.AddYears(value),
            _ => baseDate.AddDays(value) // default to days if unit unset/unrecognized
        };

        private static int? CalendarPeriodToDays(int? value, string? unit)
        {
            if (!value.HasValue) return null;
            return unit switch
            {
                "DAY" => value,
                "MONTH" => value * 30,
                "YEAR" => value * 365,
                _ => value
            };
        }
    }
}