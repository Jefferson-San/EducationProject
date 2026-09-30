namespace Education.CrossCutting.Exceptions;

/// <summary>
/// Erro de negócio lançado como exceção (casos excepcionais). O caminho normal é retornar Result;
/// o ExceptionHandlingMiddleware converte esta exceção em 400.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message) => Code = code;
}
