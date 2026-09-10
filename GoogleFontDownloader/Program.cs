using GoogleFontDownloader;
using System.CommandLine;

RootCommand rootCommand = new("@fob/client/generators:pages");
rootCommand.Add(DownloadFontCommand.Family);
rootCommand.SetAction((args, c) => new DownloadFontCommand(args).ExecuteAsync(c));
var result = rootCommand.Parse(args);
await result.InvokeAsync(cancellationToken: Cli.CancellationToken);
