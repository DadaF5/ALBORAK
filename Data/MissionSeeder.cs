using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class MissionSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (context.Missions.Any(m => m.SquadronId == null))
                return;

            var trainingPhase = await context.Phases
                .SingleAsync(p => p.Name == "Training");

            var operationalPhase = await context.Phases
                .SingleAsync(p => p.Name == "Operational");

            var missions = new List<Mission>
        {
            new Mission { Name = "CAP", Code = "CAP", PhaseId = operationalPhase.Id },
            new Mission { Name = "DACT", Code = "DACT", PhaseId = trainingPhase.Id },
            new Mission { Name = "BFM", Code = "BFM", PhaseId = trainingPhase.Id },
            new Mission { Name = "NAV", Code = "NAV", PhaseId = trainingPhase.Id },
            new Mission { Name = "A2G", Code = "A2G", PhaseId = operationalPhase.Id }
        };

            context.Missions.AddRange(missions);
            await context.SaveChangesAsync();
        }
    }
}
