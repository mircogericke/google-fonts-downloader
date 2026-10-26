using System.Collections.Specialized;
using System.CommandLine;
using System.Text.RegularExpressions;

namespace GoogleFontDownloader;

internal class DownloadFontCommand(ParseResult args)
{
    private readonly List<string> families = args.GetRequiredValue(Family);
    private readonly HttpClient http = new();

    public static readonly Argument<List<string>> Family = new("family")
    {
        Description = "The font families to download.",
        Arity = ArgumentArity.OneOrMore
    };

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Loading style..");
        var css = await DownloadStyleSheet(cancellationToken);
        Console.WriteLine("Processing style..");
        Directory.CreateDirectory("fonts");

        await using var stylesheet = new StreamWriter(Path.Join("fonts", $"fonts.css"));
        var fontUrls = new List<(string Url, string FileName)>();

        foreach (Match match in CssParser.FontFace.Matches(css))
        {
            var glyphs = match.Groups["glyphs"]?.Value ?? "unknown";
            var family = CssParser.FontFamily.Match(match.Value).Groups["value"]?.Value;
            var url = CssParser.Url.Match(match.Value).Groups["url"]?.Value;
            var style = CssParser.FontStyle.Match(match.Value).Groups["value"]?.Value ?? "normal";
            var weight = CssParser.FontWeight.Match(match.Value).Groups["value"]?.Value ?? "400";

            if (family is null || url is null)
            {
                Console.WriteLine($"Could not find family or url on {match.Value}");
                continue;
            }

            var fileName = $"{family}-{style}-{weight}-{glyphs}.woff2".ToLowerInvariant().Replace(" ", "-");
            await stylesheet.WriteLineAsync(CssParser.Url.Replace(match.Value, m =>
            {
                var group = m.Groups["value"];
                fontUrls.Add((group.Value, fileName));
                return $"url(./{fileName})";
            }));
        }

        Console.WriteLine($"Found {fontUrls.Count} font files");

        foreach (var (url, fileName) in fontUrls)
        {
            Console.WriteLine($"Downloading {fileName}");
            var stream = await http.GetStreamAsync(url, cancellationToken);
            await using var file = File.OpenWrite(Path.Join("fonts", fileName));
            await stream.CopyToAsync(file, cancellationToken);
        }

        Console.WriteLine("done.");

    }

    private async Task<string> DownloadStyleSheet(CancellationToken cancellationToken)
    {
        // use modern browser user agent, otherwise server will generate true-type fonts
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:144.0) Gecko/20100101 Firefox/144.0");
        return await http.GetStringAsync(BuildUri(), cancellationToken);
    }

    private Uri BuildUri()
    {
        var builder = new UriBuilder("https://fonts.googleapis.com/css");
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        foreach (var family in families)
            query.Add("family", family);

        builder.Query = query.ToString();

        return builder.Uri;
    }

}
