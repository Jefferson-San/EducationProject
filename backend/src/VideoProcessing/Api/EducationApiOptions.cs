namespace VideoProcessing.Api;

public class EducationApiOptions
{
    public const string SectionName = "EducationApi";

    /// <summary>Endereço interno da API. No Docker: http://education-api:8080.</summary>
    public string BaseUrl { get; set; } = "http://localhost:5214";
}

/// <summary>Chave compartilhada enviada no header X-Internal-Api-Key.</summary>
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
