namespace Education.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Raiz do Storage. No Docker: /videos (volume compartilhado). Relativo = a partir do content root.</summary>
    public string RootPath { get; set; } = "storage";

    /// <summary>
    /// Endereço público do serviço de mídia (Nginx), que serve a pasta "processed/" do mesmo volume.
    /// É a URL que o navegador do aluno usa; por isso "localhost", e não o nome do container.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:8081";

    public string ResolveRoot(string contentRootPath) => Path.GetFullPath(RootPath, contentRootPath);
}
