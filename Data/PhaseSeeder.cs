using FRAProject.Models;

namespace FRAProject.Data
{
    public class PhaseSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (context.Phases.Any())
                return;

            var phases = new List<Phase>
        {
            new Phase { Name = "Planning", Description = "Planning activities" },
            new Phase { Name = "Training", Description = "Training missions" },
            new Phase { Name = "Operational", Description = "Operational missions" },
            new Phase { Name = "Evaluation", Description = "Evaluation / check rides" }
        };

            context.Phases.AddRange(phases);
            await context.SaveChangesAsync();
        }
    }
}
