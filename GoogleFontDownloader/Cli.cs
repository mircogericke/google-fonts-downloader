namespace GoogleFontDownloader;

internal static class Cli
{
    private static readonly Lazy<CancellationTokenSource> cts = new(() =>
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => cts.Cancel();
        return cts;
    });

    public static CancellationToken CancellationToken => cts.Value.Token;
}