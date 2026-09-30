// Contrato duplicado em Education.Application/Videos/Messages/VideoUploadedMessage.cs: qualquer mudança aqui deve ser feita lá também.
namespace VideoProcessing.Contracts.Messaging;

/// <summary>
/// Publicada pela Education API após salvar o vídeo original no Storage.
/// Transporta apenas metadados — nunca o arquivo.
/// </summary>
/// <param name="VideoId">Id do vídeo no banco.</param>
/// <param name="OriginalPath">Chave relativa no Storage, ex.: "original/{videoId}/original.mp4".</param>
public sealed record VideoUploadedMessage(Guid VideoId, string OriginalPath);
