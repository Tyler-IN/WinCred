using System.Diagnostics;

namespace WinCred;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct AllocationRecord : IEquatable<AllocationRecord>
{
    public static long TimestampToTicks(long timestamp)
        => TimeSpan.TicksPerSecond == Stopwatch.Frequency
            ? timestamp
            : (long) (timestamp * (double) TimeSpan.TicksPerSecond
                      / Stopwatch.Frequency);

    public readonly void* Address;
    public readonly nuint Size;
    public readonly Type? Type;
    public readonly string? File;
    public readonly int Line;
    public readonly long Timestamp;
    public bool Freed;
    public string? FreedFile;
    public int FreedLine;
    public long FreedTimestamp;

    public readonly double Seconds
        => (double) Timestamp / Stopwatch.Frequency;

    public readonly double SecondsAgo()
        => (double) (Stopwatch.GetTimestamp() - Timestamp) / Stopwatch.Frequency;

    public readonly double FreedSeconds
        => (double) FreedTimestamp / Stopwatch.Frequency;

    public readonly double FreedSecondsAgo()
        => (double) (Stopwatch.GetTimestamp() - FreedTimestamp) / Stopwatch.Frequency;

    public AllocationRecord(void* address, nuint size, Type? type, string? file, int line)
    {
        Address = address;
        Size = size;
        Type = type;
        File = file;
        Line = line;
        Timestamp = Stopwatch.GetTimestamp();
    }

    public void Free(string? file, int line)
    {
        Freed = true;
        FreedFile = file;
        FreedLine = line;
        FreedTimestamp = Stopwatch.GetTimestamp();
    }

    public readonly override string ToString()
        => new StringBuilder().Append(in this).ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(AllocationRecord other)
        => Address == other.Address;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
        => obj is AllocationRecord other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode()
        => unchecked((int) (long) Address);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(AllocationRecord left, AllocationRecord right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AllocationRecord left, AllocationRecord right)
        => !left.Equals(right);
}

public static class AllocationRecordHelpers
{
    public static unsafe StringBuilder Append(this StringBuilder sb, in AllocationRecord r)
    {
        sb.AppendFormat($"{r.SecondsAgo():F3}s ago: ");
        if (r.Type is not null)
            sb.AppendFormat($"{r.Type.Name}, ");

        sb.AppendFormat($"{(ulong) r.Address:X}, ");
        sb.Append(r.Size);
        sb.Append(" bytes");
        if (r.File is not null)
            sb.AppendFormat($" @ {r.File}:{r.Line}");

        if (r.Freed)
        {
            sb.AppendFormat($", freed @ {r.FreedSecondsAgo():F3}s ago");
            if (r.FreedFile is not null)
                sb.AppendFormat($", {r.FreedFile}:{r.FreedLine}");
        }

        sb.Append(')');
        return sb;
    }
}