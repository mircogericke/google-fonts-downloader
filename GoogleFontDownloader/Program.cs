using System.Text.RegularExpressions;

using GoogleFontDownloader;

Dictionary<string, string[]> fonts = new()
{
	["Comfortaa"] = ["400", "500", "700"], // "italic", "bold",
	["Open Sans"] = ["400", "500", "700"],
};

var builder = new UriBuilder("https://fonts.googleapis.com/css");
var selector = string.Join("|", fonts.Select(v => $"{v.Key}:{string.Join(",", v.Value)}"));
builder.Query = $"family={selector}";

var uri = builder.Uri;

var http = new HttpClient();

Console.WriteLine("Loading style..");
// modern browser user agent, otherwise server will generate true-type fonts
http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:144.0) Gecko/20100101 Firefox/144.0");
var css = await http.GetStringAsync(uri);

Console.WriteLine("Processing style..");
Directory.CreateDirectory("fonts");

await using var stylesheet = new StreamWriter(Path.Join("fonts", "fonts.css"));
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
	var stream = await http.GetStreamAsync(url);
	await using var file = File.OpenWrite(Path.Join("fonts", fileName));
	await stream.CopyToAsync(file);
}

Console.WriteLine("done.");
