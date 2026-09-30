// Contrato duplicado em VideoProcessing/Contracts/Storage: qualquer mudança aqui deve ser feita lá também.
namespace Education.Application.Videos;

/// <summary>
/// Organização dos arquivos de vídeo no Storage, compartilhada com o Worker (que lê o original e grava o HLS).
/// </summary>
public static class VideoStorageKeys
{
    public static string OriginalFolder(Guid videoId) => $"original/{videoId}";
    public static string OriginalFile(Guid videoId) => $"{OriginalFolder(videoId)}/original.mp4";

    public static string ProcessedFolder(Guid videoId) => $"processed/{videoId}";
    public static string MasterPlaylist(Guid videoId) => $"{ProcessedFolder(videoId)}/master.m3u8";
}
