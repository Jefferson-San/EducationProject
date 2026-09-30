using Education.Domain.Common;

namespace Education.Domain.Events;

/// <summary>Vídeo salvo no Storage e registrado: precisa ser enviado para processamento.</summary>
public sealed record VideoUploadedEvent(Guid VideoId, string OriginalPath) : DomainEvent;
