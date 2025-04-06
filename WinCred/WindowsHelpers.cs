namespace WinCred;

internal static unsafe class WindowsHelpers
{
    private const string Kernel32 = "kernel32";

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const uint MEM_COMMIT = 0x1000;

    [DllImport(Kernel32, SetLastError = true)]
    private static extern nint VirtualQuery(void* lpAddress,
        out MEMORY_BASIC_INFORMATION lpBuffer, nuint dwLength);

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

    [DllImport(Kernel32, EntryPoint = "SetLastError", SetLastError = false)]
    public static extern void SetLastError(uint dwErrCode);


    [DllImport(Kernel32, EntryPoint = "GetLastError", SetLastError = false)]
    private static extern uint _GetLastError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetLastError(bool fromPInvoke = true)
        => fromPInvoke ? unchecked((uint) Marshal.GetLastWin32Error()) : _GetLastError();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClearLastError()
        => SetLastError(0);
}