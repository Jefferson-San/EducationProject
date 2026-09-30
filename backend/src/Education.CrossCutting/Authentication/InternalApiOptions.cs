namespace Education.CrossCutting.Authentication;

/// <summary>Chave compartilhada que o Worker envia no header X-Internal-Api-Key.</summary>
public class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    public string Key { get; set; } = string.Empty;

    public void Validate()
    {
        if (Key.Length < 16)
            throw new InvalidOperationException("InternalApi:Key deve ter pelo menos 16 caracteres.");
    }
}
