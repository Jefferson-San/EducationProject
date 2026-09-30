using System.Diagnostics;

namespace VideoProcessing.Processing;

/// <summary>Executa um processo externo (ffmpeg/ffprobe) sem shell, com timeout e cancelamento.</summary>
internal static class ProcessRunner
{
    private const int ErrorTailLines = 15;

    public sealed record Result(int ExitCode, string StandardOutput, string ErrorTail, bool TimedOut);

    public static async Task<Result> RunAsync(string fileName, IEnumerable<string> arguments, string? workingDirectory,
        TimeSpan timeout, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? string.Empty
        };
        // ArgumentList: cada argumento vai separado, sem precisar escapar aspas/espaços em caminhos.
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };

        // O FFmpeg escreve logs no stderr; guardamos só o final para compor a mensagem de erro.
        var errorTail = new Queue<string>();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            lock (errorTail)
            {
                errorTail.Enqueue(e.Data);
                if (errorTail.Count > ErrorTailLines) errorTail.Dequeue();
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        var standardOutput = process.StandardOutput.ReadToEndAsync(CancellationToken.None);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { /* já terminou */ }
            if (cancellationToken.IsCancellationRequested) throw;
            return new Result(-1, string.Empty, JoinTail(errorTail), TimedOut: true);
        }

        return new Result(process.ExitCode, await standardOutput, JoinTail(errorTail), TimedOut: false);
    }

    private static string JoinTail(Queue<string> lines)
    {
        lock (lines) return string.Join(Environment.NewLine, lines);
    }
}
