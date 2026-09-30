namespace VideoProcessing.Storage;

/// <summary>
/// Storage do Worker. O FFmpeg trabalha com arquivos em disco, então a abstração devolve caminhos locais.
/// Com um Object Storage (S3/Blob), a implementação baixaria o original e enviaria a saída.
/// </summary>
public interface IFileStorage
{
    /// <summary>Caminho absoluto no disco para uma chave relativa (ex.: "original/{id}/original.mp4").</summary>
    string GetLocalPath(string key);

    /// <summary>Remove uma pasta inteira. Não falha se não existir.</summary>
    void DeleteFolder(string key);

    /// <summary>Move uma pasta, substituindo o destino se já existir.</summary>
    void MoveFolder(string sourceKey, string destinationKey);
}
