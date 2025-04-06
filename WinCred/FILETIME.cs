namespace WinCred;

[PublicAPI, StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public struct FILETIME
    : IFormattable,
        IEquatable<FILETIME>,
        IComparable<FILETIME>,
        IEquatable<DateTime>,
        IComparable<DateTime>,
        IEquatable<DateTimeOffset>,
        IComparable<DateTimeOffset>,
        IComparable,
        IConvertible
{
    private uint Low;
    private uint High;

    private long Ticks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ((long) High << 32) | Low;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime ToDateTime()
        => DateTime.FromFileTimeUtc(Ticks);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTimeOffset ToDateTimeOffset()
        => ToDateTime();

    public static FILETIME FromDateTime(DateTime dateTime)
    {
        var fileTime = dateTime
            .ToUniversalTime()
            .ToFileTimeUtc();
        return new()
        {
            Low = (uint) (fileTime & 0xFFFFFFFF),
            High = (uint) ((fileTime >> 32) & 0xFFFFFFFF)
        };
    }

    public static FILETIME FromDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        var fileTime = dateTimeOffset.UtcDateTime.ToFileTimeUtc();
        return new()
        {
            Low = (uint) (fileTime & 0xFFFFFFFF),
            High = (uint) ((fileTime >> 32) & 0xFFFFFFFF)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DateTime(FILETIME fileTime)
        => fileTime.ToDateTime();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FILETIME(DateTime dateTime)
        => FromDateTime(dateTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DateTimeOffset(FILETIME fileTime)
        => fileTime.ToDateTimeOffset();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FILETIME(DateTimeOffset dateTimeOffset)
        => FromDateTimeOffset(dateTimeOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FILETIME(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        => new() {Low = unchecked((uint) ft.dwLowDateTime), High = unchecked((uint) ft.dwHighDateTime)};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Runtime.InteropServices.ComTypes.FILETIME(FILETIME ft)
        => new() {dwLowDateTime = unchecked((int) ft.Low), dwHighDateTime = unchecked((int) ft.High)};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(FILETIME other)
        => Low == other.Low && High == other.High;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(DateTime other)
        => ToDateTime().Equals(other);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(DateTimeOffset other)
        => ToDateTimeOffset().Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(FILETIME other)
        => Ticks.CompareTo(other.Ticks);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(DateTime other)
        => ToDateTime().CompareTo(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(DateTimeOffset other)
        => ToDateTimeOffset().CompareTo(other);

    public override string ToString()
    {
        try
        {
            return $"{ToDateTime():o}";
        }
        catch
        {
            return $"(FILETIME [0x{Low:X8}, 0x{High:X8}])";
        }
    }


    public string ToString(IFormatProvider provider)
    {
        try
        {
            return $"{ToDateTime(provider):o}";
        }
        catch
        {
            return $"(FILETIME [0x{Low:X8}, 0x{High:X8}])";
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format)
        => ToString(format, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format)) format = "G";
        return format switch
        {
            "G" => ToString(),
            _ => ToDateTime().ToString(format, formatProvider)
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            FILETIME other => CompareTo(other),
            DateTime dateTime => CompareTo(dateTime),
            DateTimeOffset dateTimeOffset => CompareTo(dateTimeOffset),
            _ => 1
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypeCode GetTypeCode()
        => TypeCode.Object;

    public DateTime ToDateTime(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static DateTime Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToDateTime(provider);

        return Helper(ToDateTime(), provider);
    }

    public object ToType(Type conversionType, IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static object Helper<T>(T d, Type conversionType, IFormatProvider provider)
            where T : IConvertible
            => d.ToType(conversionType, provider);

        return Helper(ToDateTime(), conversionType, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => HashCode.Combine(Low, High);


    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToBoolean(provider);

        return Helper(Ticks, provider);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToByte(provider);

        return Helper(Ticks, provider);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToChar(provider);

        return Helper(Ticks, provider);
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static decimal Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToDecimal(provider);

        return Helper(Ticks, provider);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToDouble(provider);

        return Helper(Ticks, provider);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static short Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToInt16(provider);

        return Helper(Ticks, provider);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToInt32(provider);

        return Helper(Ticks, provider);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToInt64(provider);

        return Helper(Ticks, provider);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static sbyte Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToSByte(provider);

        return Helper(Ticks, provider);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToSingle(provider);

        return Helper(Ticks, provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ushort Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToUInt16(provider);

        return Helper(Ticks, provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToUInt32(provider);

        return Helper(Ticks, provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Helper<T>(T d, IFormatProvider provider)
            where T : IConvertible
            => d.ToUInt64(provider);

        return Helper(Ticks, provider);
    }
}