namespace GoogleFontDownloader;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

internal partial class CssParser
{
	[GeneratedRegex(@"/\*\s*(?<glyphs>.+?)\s*\*/\s*@font-face\s*{.+?}", RegexOptions.Singleline)]
	public static partial Regex FontFace { get; }

	[GeneratedRegex("""font-family:\s*(["'])(?<value>.+?)\1\s*;""")]
	public static partial Regex FontFamily { get; }

	[GeneratedRegex(@"font-weight:\s* (?<value>.+?)\s*;")]
	public static partial Regex FontWeight { get; }

	[GeneratedRegex(@"font-style:\s* (?<value>.+?)\s*;")]
	public static partial Regex FontStyle { get; }

	[GeneratedRegex(@"url\((?<value>.+?)\)")]
	public static partial Regex Url { get; }
}



