using Education.Domain.Interfaces.Videos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Education.Infrastructure.Storage;

/// <summary>Implementação do MVP: disco local / Docker Volume.</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly string _publicBaseUrl;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        _root = options.Value.ResolveRoot(environment.ContentRootPath);
        _publicBaseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        Directory.CreateDirectory(_root);
    }

    public string GetPublicUrl(string key) => $"{_publicBaseUrl}/{key.TrimStart('/')}";

    public async Task SaveAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = Resolve(key);
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        else if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Converte a chave em caminho absoluto, impedindo sair da raiz (ex.: "../").</summary>
    private string Resolve(string key)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, key));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException($"Chave de storage inválida: {key}", nameof(key));
        return fullPath;
    }
}
