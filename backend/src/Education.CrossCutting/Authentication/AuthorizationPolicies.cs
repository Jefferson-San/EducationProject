namespace Education.CrossCutting.Authentication;

/// <summary>Políticas usadas pelos endpoints com .RequireAuthorization(...).</summary>
public static class AuthorizationPolicies
{
    /// <summary>Usuário autenticado por JWT com role Teacher.</summary>
    public const string Teacher = "Teacher";

    /// <summary>Chamadas do Video Processing Worker (header X-Internal-Api-Key).</summary>
    public const string InternalApi = "InternalApi";
}
