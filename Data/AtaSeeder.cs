using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AtaCategorySeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AtaCategory>().AnyAsync())
                return;

            var categories = new List<AtaCategory>
            {
                new() { Code = "GEN",   Name = "Aircraft General",  SortOrder = 1, IsActive = true },
                new() { Code = "AFS",   Name = "Airframe Systems",  SortOrder = 2, IsActive = true },
                new() { Code = "STRUC", Name = "Structure",         SortOrder = 3, IsActive = true },
                new() { Code = "PWR",   Name = "Power Plant",       SortOrder = 4, IsActive = true },
            };

            await context.Set<AtaCategory>().AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    public class AtaSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<Ata>().AnyAsync())
                return;

            await AtaCategorySeeder.SeedAsync(context);

            var genId = await context.Set<AtaCategory>().Where(c => c.Code == "GEN").Select(c => c.Id).SingleAsync();
            var airId = await context.Set<AtaCategory>().Where(c => c.Code == "AFS").Select(c => c.Id).SingleAsync();
            var strId = await context.Set<AtaCategory>().Where(c => c.Code == "STRUC").Select(c => c.Id).SingleAsync();
            var pwrId = await context.Set<AtaCategory>().Where(c => c.Code == "PWR").Select(c => c.Id).SingleAsync();

            // Full chapter list per ATA iSpec 2200 (chapter level only — see
            // ATA_Chapters.pdf). Category assigned per the document's own
            // section headers.
            var chapters = new (string Code, string Name, int CategoryId)[]
            {
                // AIRCRAFT GENERAL
                ("05", "Time Limits / Maintenance Checks", genId),
                ("06", "Dimensions and Areas", genId),
                ("07", "Lifting and Shoring", genId),
                ("08", "Leveling and Weighing", genId),
                ("09", "Towing and Taxi", genId),
                ("10", "Parking, Mooring, Storage and Return to Service", genId),
                ("11", "Placards and Markings", genId),
                ("12", "Servicing — Routine Maintenance", genId),
                ("18", "Vibration and Noise Analysis (Helicopter Only)", genId),
                ("89", "Flight Test Installation", genId),

                // AIRFRAME SYSTEMS
                ("20", "Standard Practices — Airframe", airId),
                ("21", "Air Conditioning and Pressurization", airId),
                ("22", "Auto Flight", airId),
                ("23", "Communications", airId),
                ("24", "Electrical Power", airId),
                ("25", "Equipment / Furnishings", airId),
                ("26", "Fire Protection", airId),
                ("27", "Flight Controls", airId),
                ("28", "Fuel", airId),
                ("29", "Hydraulic Power", airId),
                ("30", "Ice and Rain Protection", airId),
                ("31", "Indicating / Recording System", airId),
                ("32", "Landing Gear", airId),
                ("33", "Lights", airId),
                ("34", "Navigation", airId),
                ("35", "Oxygen", airId),
                ("36", "Pneumatic", airId),
                ("37", "Vacuum", airId),
                ("38", "Water/Waste", airId),
                ("39", "Electrical / Electronic Panels and Multipurpose Components", airId),
                ("40", "Multisystem", airId),
                ("41", "Water Ballast", airId),
                ("42", "Integrated Modular Avionics", airId),
                ("44", "Cabin Systems", airId),
                ("45", "Diagnostic and Maintenance System", airId),
                ("46", "Information Systems", airId),
                ("47", "Nitrogen Generation System", airId),
                ("48", "In Flight Fuel Dispensing", airId),
                ("49", "Airborne Auxiliary Power", airId),
                ("50", "Cargo and Accessory Compartments", airId),

                // STRUCTURE
                ("51", "Standard Practices and Structures — General", strId),
                ("52", "Doors", strId),
                ("53", "Fuselage", strId),
                ("54", "Nacelles / Pylons", strId),
                ("55", "Stabilizers", strId),
                ("56", "Windows", strId),
                ("57", "Wings", strId),

                // POWER PLANT (includes Propeller/Rotor chapters, and 91/92
                // per the document's own grouping)
                ("60", "Standard Practices — Propeller/Rotor", pwrId),
                ("61", "Propellers", pwrId),
                ("62", "Rotor(s)", pwrId),
                ("63", "Rotor Drive(s)", pwrId),
                ("64", "Tail Rotor", pwrId),
                ("65", "Tail Rotor Drive", pwrId),
                ("66", "Folding Blades / Pylon", pwrId),
                ("67", "Rotors Flight Control", pwrId),
                ("70", "Standard Practices — Engine", pwrId),
                ("71", "Power Plant", pwrId),
                ("72", "Engine — Reciprocating", pwrId),
                ("73", "Engine — Fuel and Control", pwrId),
                ("74", "Ignition", pwrId),
                ("75", "Bleed Air", pwrId),
                ("76", "Engine Controls", pwrId),
                ("77", "Engine Indicating", pwrId),
                ("78", "Exhaust", pwrId),
                ("79", "Oil", pwrId),
                ("80", "Starting", pwrId),
                ("81", "Turbines (Reciprocating Engines)", pwrId),
                ("82", "Engine Water Injection", pwrId),
                ("83", "Accessory Gearboxes", pwrId),
                ("84", "Propulsion Augmentation", pwrId),
                ("85", "Fuel Cell Systems", pwrId),
                ("91", "Charts", pwrId),
                ("92", "Electrical System Installation", pwrId),
            };

            var entities = chapters
                .Select((c, i) => new Ata
                {
                    Code = c.Code,
                    Name = c.Name,
                    AtaCategoryId = c.CategoryId,
                    SortOrder = (byte)Math.Min(i, 255),
                    IsActive = true
                })
                .ToList();

            await context.Set<Ata>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }
}