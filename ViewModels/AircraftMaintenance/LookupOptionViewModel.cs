namespace FRAProject.ViewModels.AircraftMaintenance
{
    // ⚠ ASSUMED SHAPE — LookupOptionViewModel.cs was referenced by your
    // existing WorkOrderFormViewModel but not provided this session.
    // Assumed: Id + Label. If your real class differs (e.g. different
    // property name, or it already exists elsewhere in the project),
    // DELETE this file and adjust WorkOrdersController.cs's
    // `new LookupOptionViewModel { Id = ..., Label = ... }` calls to match
    // your actual property names instead.
    public class LookupOptionViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}