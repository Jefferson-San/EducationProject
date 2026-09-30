namespace Education.Domain.Interfaces.Videos;

/// <summary>
/// Storage dos arquivos de vídeo. As chaves são relativas (ex.: "original/{id}/original.mp4"),
/// então a implementação pode ser trocada por S3/Azure Blob/MinIO sem mudar quem usa.
/// </summary>
public interface IFileStorage
{
    Task SaveAsync(string key, Stream content, CancellationToken cancellationToken = default);

    /// <summary>Remove um arquivo ou uma "pasta" inteira (prefixo). Não falha se não existir.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL pública (acessada pelo navegador) de um arquivo publicável, ex.: o master.m3u8.
    /// No MVP aponta para o serviço de mídia (Nginx); com S3/Blob seria a URL do bucket ou do CDN.
    /// </summary>
    string GetPublicUrl(string key);
}
