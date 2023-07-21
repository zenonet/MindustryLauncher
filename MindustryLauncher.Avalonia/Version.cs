using System;
using System.Text.RegularExpressions;

namespace MindustryLauncher;

public partial struct Version : IComparable<Version>
{
    public int Major { get; set; }

    public int Minor { get; set; }

    public Version(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }

    #region Static Fields

    public static readonly Version MinVersionWithBuild = new(58, 0);
    
    public static readonly Version MinVersionWithNewBuildName = new(90, 0);

    #endregion


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

    public static bool operator >(Version left, Version rigth)
    {
        int majorDifference = left.Major - rigth.Major;
        int minorDifference = left.Minor - rigth.Minor;

        return majorDifference == 0 ? minorDifference > 0 : majorDifference > 0;
    }

    public static bool operator <(Version left, Version rigth)
    {
        int majorDifference = left.Major - rigth.Major;
        int minorDifference = left.Minor - rigth.Minor;

        return majorDifference == 0 ? minorDifference < 0 : majorDifference < 0;
    }

    public static bool operator ==(Version left, Version rigth)
    {
        return left.Major == rigth.Major && left.Minor == rigth.Minor;
    }

    public static bool operator !=(Version left, Version rigth)
    {
        return !(left == rigth);
    }

    public int CompareTo(Version other)
    {
        int majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;
        return Minor.CompareTo(other.Minor);
    }
}