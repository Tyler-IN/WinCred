namespace WinCred;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public struct FILETIME
{
    private uint Low;
    private uint High;

    public static implicit operator DateTime(FILETIME fileTime)
        => DateTime.FromFileTime(((long) fileTime.High << 32) | fileTime.Low);

    public static implicit operator FILETIME(DateTime dateTime)
        => new()
        {
            Low = (uint) (dateTime.ToFileTime() & 0xFFFFFFFF),
            High = (uint) ((dateTime.ToFileTime() >> 32) & 0xFFFFFFFF)
        };

    public static implicit operator DateTimeOffset(FILETIME fileTime)
        => DateTimeOffset.FromFileTime(((long) fileTime.High << 32) | fileTime.Low);

    public static implicit operator FILETIME(DateTimeOffset dateTimeOffset)
        => new()
        {
            Low = (uint) (dateTimeOffset.ToFileTime() & 0xFFFFFFFF),
            High = (uint) ((dateTimeOffset.ToFileTime() >> 32) & 0xFFFFFFFF)
        };
}