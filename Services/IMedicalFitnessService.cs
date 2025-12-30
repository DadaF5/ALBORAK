using FRAProject.Models;
using System;
using System.Threading.Tasks;

namespace FRAProject.Services.Medical
{
    public interface IMedicalFitnessService
    {
        Task<MedicalFitnessResult> EvaluateAsync(
            int crewMemberId,
            DateTime referenceDate);
    }
}

