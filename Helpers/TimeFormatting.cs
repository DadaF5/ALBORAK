namespace FRAProject.Helpers
{
    public class TimeFormatting
    {
        // Convert minutes (nullable) to "H:MM" string, or "-" if null
        public static string FormatMinutesAsHMM(int? minutes)
        {
            if (!minutes.HasValue) return "-";
            var m = minutes.Value;
            if (m < 0) return "-";
            var h = m / 60;
            var mm = m % 60;
            return $"{h}:{mm:D2}";
        }
    }
}
