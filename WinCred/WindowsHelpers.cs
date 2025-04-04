namespace WinCred;

public static unsafe class WindowsHelpers
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint VirtualQuery(void* lpAddress,
        out MEMORY_BASIC_INFORMATION lpBuffer, nuint dwLength);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const uint MEM_COMMIT = 0x1000;

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private readonly struct MEMORY_BASIC_INFORMATION
    {
        public readonly nint BaseAddress;
        public readonly nint AllocationBase;
        public readonly uint AllocationProtect;
        public readonly nint RegionSize;
        public readonly uint State;
        public readonly uint Protect;
        public readonly uint Type;
    }

    public static bool IsPageValid(void* address)
    {
        return VirtualQuery(address, out var mbi,
                   (nuint) Unsafe.SizeOf<MEMORY_BASIC_INFORMATION>()) != default
               && (mbi.State & MEM_COMMIT) != 0;
    }
}