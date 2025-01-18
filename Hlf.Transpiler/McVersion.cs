namespace Hlf.Transpiler;

public struct McVersion(int major, int minor, int patch = 0)
{
    public int Major { get; set; } = major;
    public int Minor { get; set; } = minor;
    public int Patch { get; set; } = patch;
    
    public static McVersion OneDot(int minor, int patch = 0) => new (1, minor, patch);


    public override string ToString()
    {
        if (Patch == 0) return $"{Major}.{Minor}";
        return $"{Major}.{Minor}.{Patch}";
    }

    private bool Equals(McVersion other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((McVersion) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch);
    }

    public static bool operator <(McVersion left, McVersion right)
    {
        if(left.Major != right.Major) return left.Major < right.Major;
        if(left.Minor != right.Minor) return left.Minor < right.Minor;
        if(left.Patch != right.Patch) return left.Patch < right.Patch;
        return false;
    }

    public static bool operator >(McVersion left, McVersion right)
    {
        if(left.Major != right.Major) return left.Major > right.Major;
        if(left.Minor != right.Minor) return left.Minor > right.Minor;
        if(left.Patch != right.Patch) return left.Patch > right.Patch;
        return false;
    }
}