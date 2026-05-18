using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace FRAProject.Services
{
    // ════════════════════════════════════════════════════════════════
    //  RESULT — returned by SaveFileAsync
    // ════════════════════════════════════════════════════════════════
    public class FileUploadResult
    {
        public bool    Success         { get; init; }
        public string? ErrorMessage    { get; init; }
        public string? FilePath        { get; init; }
        public string? FileName        { get; init; }
        public long    FileSize        { get; init; }
        public string? MimeType        { get; init; }

        public static FileUploadResult Ok(
            string filePath, string fileName, long fileSize, string mimeType) =>
            new() { Success = true, FilePath = filePath,
                    FileName = fileName, FileSize = fileSize, MimeType = mimeType };

        public static FileUploadResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }

    // ════════════════════════════════════════════════════════════════
    //  INTERFACE
    // ════════════════════════════════════════════════════════════════
    /// <summary>
    /// Physical file I/O for immatriculation documents.
    /// Knows nothing about EF, dossier business logic, or controllers.
    ///
    /// Storage convention:
    ///   D:\2BAFRA\Uploads\Immatriculation\{DossierId}\{DocCode}_{FileName}
    ///
    /// Register in Program.cs:
    ///   builder.Services.AddScoped&lt;IFileUploadService, FileUploadService&gt;();
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Save a single uploaded file to the immatriculation upload folder.
        /// Validates size against maxFileSizeMb before writing.
        /// </summary>
        Task<FileUploadResult> SaveFileAsync(
            int        dossierId,
            string     docTypeCode,
            int?       maxFileSizeMb,
            IFormFile  file);

        /// <summary>
        /// Soft-delete on disk — renames file to .deleted extension.
        /// Physical file is kept for audit; only the DB IsActive flag
        /// controls visibility.
        /// </summary>
        void MarkFileDeleted(string filePath);

        /// <summary>
        /// Build the physical folder path for a dossier.
        /// Creates the folder if it does not exist.
        /// </summary>
        string EnsureDossierFolder(int dossierId);

        /// <summary>
        /// Build a safe file name: "{docTypeCode}_{originalFileName}"
        /// Strips unsafe characters from the original name.
        /// </summary>
        string BuildSafeFileName(string docTypeCode, string originalFileName);
    }

    // ════════════════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ════════════════════════════════════════════════════════════════
    public class FileUploadService : IFileUploadService
    {
        // Upload root — D:\2BAFRA\Uploads\Immatriculation\
        private const string UploadRoot =
            @"D:\2BAFRA\Uploads\Immatriculation\";

        private static readonly FileExtensionContentTypeProvider
            _mimeProvider = new();

        // ── SaveFileAsync ─────────────────────────────────────────────
        public async Task<FileUploadResult> SaveFileAsync(
            int       dossierId,
            string    docTypeCode,
            int?      maxFileSizeMb,
            IFormFile file)
        {
            // ── Validate ─────────────────────────────────────────────
            if (file == null || file.Length == 0)
                return FileUploadResult.Fail("Le fichier est vide.");

            if (maxFileSizeMb.HasValue &&
                file.Length > maxFileSizeMb.Value * 1_048_576)
                return FileUploadResult.Fail(
                    $"Le fichier depasse la taille maximale autorisee " +
                    $"({maxFileSizeMb} Mo).");

            // ── Build path ───────────────────────────────────────────
            var folder   = EnsureDossierFolder(dossierId);
            var safeName = BuildSafeFileName(docTypeCode, file.FileName);
            var filePath = Path.Combine(folder, safeName);

            // ── Write to disk ────────────────────────────────────────
            try
            {
                await using var stream = new FileStream(
                    filePath, FileMode.Create, FileAccess.Write);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return FileUploadResult.Fail(
                    $"Erreur lors de l'enregistrement du fichier : {ex.Message}");
            }

            // ── Detect MIME ──────────────────────────────────────────
            _mimeProvider.TryGetContentType(file.FileName, out var mime);

            return FileUploadResult.Ok(
                filePath:  filePath,
                fileName:  file.FileName,
                fileSize:  file.Length,
                mimeType:  mime ?? "application/octet-stream");
        }

        // ── MarkFileDeleted ───────────────────────────────────────────
        // Renames to .deleted so it's invisible but auditable.
        // Physical deletion is a separate admin operation if needed.
        public void MarkFileDeleted(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var deletedPath = filePath + ".deleted";
            try { File.Move(filePath, deletedPath, overwrite: true); }
            catch { /* log if needed — non-critical */ }
        }

        // ── EnsureDossierFolder ───────────────────────────────────────
        public string EnsureDossierFolder(int dossierId)
        {
            var folder = Path.Combine(UploadRoot, dossierId.ToString());
            Directory.CreateDirectory(folder);
            return folder;
        }

        // ── BuildSafeFileName ─────────────────────────────────────────
        // "DOC01" + "mon fichier (2).pdf" → "DOC01_mon_fichier_2_.pdf"
        public string BuildSafeFileName(string docTypeCode, string originalFileName)
        {
            var name = Path.GetFileNameWithoutExtension(originalFileName);
            var ext  = Path.GetExtension(originalFileName).ToLower();

            // Replace unsafe chars with underscore
            var safeName = string.Concat(
                name.Select(c => Path.GetInvalidFileNameChars().Contains(c)
                    || c == ' ' ? '_' : c));

            return $"{docTypeCode}_{safeName}{ext}";
        }
    }
}
