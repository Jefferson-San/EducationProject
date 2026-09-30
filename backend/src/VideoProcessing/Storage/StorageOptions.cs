namespace VideoProcessing.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Raiz do Storage. No Docker: /videos (mesmo volume da API). Relativo = a partir do content root.</summary>
    public string RootPath { get; set; } = "storage";
}
