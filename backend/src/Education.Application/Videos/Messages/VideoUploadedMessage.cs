// Contrato duplicado em VideoProcessing/Contracts/Messaging: qualquer mudança aqui deve ser feita lá também.
namespace Education.Application.Videos.Messages;

/// <summary>
/// Mensagem de integração enviada ao Video Processing Worker (fila "video.uploaded").
/// Transporta apenas metadados — nunca o arquivo.
/// </summary>
/// <param name="OriginalPath">Chave relativa no Storage, ex.: "original/{videoId}/original.mp4".</param>
public sealed record VideoUploadedMessage(Guid VideoId, string OriginalPath);
