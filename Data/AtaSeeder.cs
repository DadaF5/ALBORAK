using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AtaSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            await AtaCategorySeeder.SeedAsync(context);

            var genId = await context.Set<AtaCategory>()
                .Where(c => c.Code == "GEN")
                .Select(c => c.Id)
                .SingleOrDefaultAsync();

            var airId = await context.Set<AtaCategory>()
                .Where(c => c.Code == "AFS")
                .Select(c => c.Id)
                .SingleOrDefaultAsync();

            var strId = await context.Set<AtaCategory>()
                .Where(c => c.Code == "STRUC")
                .Select(c => c.Id)
                .SingleOrDefaultAsync();

            var pwrId = await context.Set<AtaCategory>()
                .Where(c => c.Code == "PWR")
                .Select(c => c.Id)
                .SingleOrDefaultAsync();

            if (genId == 0 || airId == 0 || strId == 0 || pwrId == 0)
                throw new InvalidOperationException("AtaSeeder prerequisites missing: GEN/AFS/STRUC/PWR categories.");

            var chapters = new (string Code, string Name, int CategoryId)[]
            {
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

                ("51", "Standard Practices and Structures — General", strId),
                ("52", "Doors", strId),
                ("53", "Fuselage", strId),
                ("54", "Nacelles / Pylons", strId),
                ("55", "Stabilizers", strId),
                ("56", "Windows", strId),
                ("57", "Wings", strId),

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

            var existingCodes = await context.Set<Ata>()
                .Select(a => a.Code)
                .ToListAsync();

            var entities = chapters
                .Where(c => !existingCodes.Contains(c.Code))
                .Select((c, i) => new Ata
                {
                    Code = c.Code,
                    Name = c.Name,
                    AtaCategoryId = c.CategoryId,
                    SortOrder = (byte)Math.Min(i + 1, 255),
                    IsActive = true
                })
                .ToList();

            if (entities.Any())
            {
                await context.Set<Ata>().AddRangeAsync(entities);
                await context.SaveChangesAsync();
            }
        }
    }
}