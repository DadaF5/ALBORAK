using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _uow;

        public HomeController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ── DASHBOARD ────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var vm = new SettingsDashboardVm
            {
                // ── DONNÉES DE BASE ───────────────────────────────────────
                DonneesDeBase =
                [
                    await BuildStatAsync(
                        title:      "Groupes d'aéronefs",
                        controller: "AcMainGroups",
                        icon:       "fa-layer-group",
                        section:    "Données de base",
                        total:      await _uow.AcMainGroups.CountAsync(_ => true),
                        active:     await _uow.AcMainGroups.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Types d'aéronefs",
                        controller: "AcTypes",
                        icon:       "fa-fighter-jet",
                        section:    "Données de base",
                        total:      await _uow.AcTypes.CountAsync(_ => true),
                        active:     await _uow.AcTypes.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Versions d'aéronefs",
                        controller: "AircraftVersions",
                        icon:       "fa-plane",
                        section:    "Données de base",
                        total:      await _uow.AircraftVersions.CountAsync(_ => true),
                        active:     await _uow.AircraftVersions.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Catégories",
                        controller: "AcCategory",
                        icon:       "fa-tags",
                        section:    "Données de base",
                        total:      await _uow.AcCategories.CountAsync(_ => true),
                        active:     await _uow.AcCategories.CountAsync(x => x.IsActive)
                    )
                    ,
                    await BuildStatAsync(
                        title:      "Rôles et missions",
                        controller: "MissionRole",
                        icon:       "fa-crosshairs",
                        section:    "Données de base",
                        total:      await _uow.MissionRoles.CountAsync(_ => true),
                        active:     await _uow.MissionRoles.CountAsync(x => x.IsActive)
                    )
                    ,
                    await BuildStatAsync(
                        title:      "Statuts aéronef",
                        controller: "AcStatusType",
                        icon:       "fa-circle-dot",
                        section:    "Données de base",
                        total:      await _uow.AcStatusTypes.CountAsync(_ => true),
                        active:     await _uow.AcStatusTypes.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Bases aériennes",
                        controller: "Base",
                        icon:       "fa-map-marker-alt",
                        section:    "Données de base",
                        total:      await _uow.Bases.CountAsync(_ => true),
                        active:     await _uow.Bases.CountAsync(x => x.IsActive)
                    ),
                ],

                // ── RÉFÉRENTIELS ──────────────────────────────────────────
                Referentiels =
                [
                    await BuildStatAsync(
                        title:      "Pays",
                        controller: "Country",
                        icon:       "fa-globe",
                        section:    "Référentiels",
                        total:      await _uow.Countries.CountAsync(_ => true),
                        active:     await _uow.Countries.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Autorités d'emploi",
                        controller: "EmployingAuthority",
                        icon:       "fa-shield-alt",
                        section:    "Référentiels",
                        total:      await _uow.EmployingAuthorities.CountAsync(_ => true),
                        active:     await _uow.EmployingAuthorities.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Constructeurs",
                        controller: "AircraftManufacturers",
                        icon:       "fa-industry",
                        section:    "Référentiels",
                        total:      await _uow.AircraftManufacturers.CountAsync(_ => true),
                        active:     await _uow.AircraftManufacturers.CountAsync(x => x.IsActive)
                    ),
                    await BuildStatAsync(
                        title:      "Types CdN",
                        controller: "CdnDocType",
                        icon:       "fa-certificate",
                        section:    "Référentiels",
                        total:      await _uow.CdnDocTypes.CountAsync(_ => true),
                        active:     await _uow.CdnDocTypes.CountAsync(x => x.IsActive)
                    ),
                ]
                ,

                // ── IMMATRICULATION ───────────────────────────────────────
                Immatriculation =
                [
                    await BuildStatAsync(
                        title:      "Types de documents",
                        controller: "ImmatriculationDocType",
                        icon:       "fa-file-alt",
                        section:    "Immatriculation",
                        total:      await _uow.ImmatriculationDocTypes.CountAsync(_ => true),
                        active:     await _uow.ImmatriculationDocTypes.CountAsync(x => x.IsActive)
                    )
                    ,
                    await BuildStatAsync(
                        title:      "Dossiers DAM",
                        controller: "Dossier",
                        icon:       "fa-folder-open",
                        section:    "Immatriculation",
                        total:      await _uow.Dossiers.CountAsync(_ => true),
                        active:     await _uow.Dossiers.CountAsync(x => x.IsActive)
                    )
                    ,
                ]
            };

            return View(vm);
        }

        // ── HELPER ───────────────────────────────────────────────────────
        private static Task<LookupTableStatVm> BuildStatAsync(
            string title, string controller, string icon,
            string section, int total, int active) =>
            Task.FromResult(new LookupTableStatVm
            {
                Title = title,
                Controller = controller,
                Icon = icon,
                Section = section,
                TotalCount = total,
                ActiveCount = active
            });

        // This action is used for access denied redirection from cookie options
        // No admin can see the access denied message
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}