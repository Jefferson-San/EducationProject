namespace Education.Domain.Interfaces;

public interface IUnitOfWork
{
    /// <exception cref="Common.ConcurrencyException">Outro processo alterou o mesmo registro (concorrência otimista).</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
