using System;
using System.Text.RegularExpressions;

namespace MindustryLauncher;

public partial class Version
{
    public int Major { get; set; }

    public int Minor { get; set; }
    
    public Version(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }


    public static Version Parse(string input)
    {
        Match match = ParserRegex().Match(input);

        if (!match.Success)
            throw new FormatException("The provided mindustry version was formatted incorrectly");

        return new(
            major: int.Parse(match.Groups[1].Value),
            minor: match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0
        );
    }

    [GeneratedRegex("^v?(\\d+)(?:\\.([1-9]+))?$")]
    private static partial Regex ParserRegex();

    public override string ToString()
    {
        return Minor == 0 ? Major.ToString() : $"{Major}.{Minor}";
    }
}