using Microsoft.Extensions.Options;

namespace VideoProcessing.Storage;

/// <summary>Implementação do MVP: Docker Volume compartilhado com a Education API.</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        _root = Path.GetFullPath(options.Value.RootPath, environment.ContentRootPath);
        Directory.CreateDirectory(_root);
    }

    public string GetLocalPath(string key)
    {
        // Impede que uma chave como "../" saia da raiz do Storage.
        var fullPath = Path.GetFullPath(Path.Combine(_root, key));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException($"Chave de storage inválida: {key}", nameof(key));
        return fullPath;
    }

    public void DeleteFolder(string key)
    {
        var path = GetLocalPath(key);
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }

    public void MoveFolder(string sourceKey, string destinationKey)
    {
        var source = GetLocalPath(sourceKey);
        var destination = GetLocalPath(destinationKey);

        if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        // Mesmo volume: é um rename, então a pasta final aparece de uma vez, já completa.
        Directory.Move(source, destination);
    }
}
