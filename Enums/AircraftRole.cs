namespace FRAProject.Enums
{
    public enum AircraftRole
    {
        Captain,
        Copilot,
        WeaponOfficer,
        AvionicsEngineer,
        Instructor,
        Student,
        FunctionalCheckPilot,
        Mechanic,

        // NEW (Batch 15, 2026-08-30) — appended, not inserted, per Dadda's
        // confirmed choice. AircraftRole is stored as its underlying int in
        // the database (no [Flags], no explicit values assigned anywhere in
        // the real enum), so every existing value above keeps its ordinal
        // (Captain=0 ... Mechanic=7) and every row already written against
        // this enum is unaffected. Passenger=8.
        //
        // Closes the real gap flagged since the legacy CIPL_FlyingProgram
        // page was shared: legacy's "Add Sortie to ODV" assigns Pax1/Pax2
        // in the same one-step submit as Captain/Co-Pilot, and this enum
        // had no value to represent them — SortieCrew.AircraftRole is
        // [Required], so a passenger could only have been saved under a
        // misleading existing value (e.g. Student, Mechanic) without this.
        Passenger,

        // NEW (Batch 16, 2026-08-30) — three more appended values, per
        // Dadda's confirmed real crew positions for the C130 (Navigator/
        // Combat Systems Officer, Flight Engineer, Loadmaster) and F16
        // (Flight Engineer also applies there, as one of three possible
        // second-seat roles alongside Copilot/WeaponOfficer). Again
        // appended, not inserted — every prior ordinal (Captain=0 ...
        // Passenger=8) is unaffected.
        //
        // "Navigator (or Combat Systems Officer)" is deliberately ONE
        // value, not two — per Dadda's choice, this is one seat/role by
        // whichever name a given squadron uses for it, not two distinct
        // crew positions.
        Navigator,
        FlightEngineer,
        Loadmaster
    }
}
