#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace WinCred;

[PublicAPI]
[ExcludeFromCodeCoverage]
public unsafe struct Pointer<T> : IEquatable<Pointer<T>>
{
    public T* Value;

    public readonly ref T Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<T>(Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer(T* value) => Value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(Pointer<T> ptr) => ptr.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Pointer<T>(T* pointer) => new(pointer);

    // dereference operator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Pointer<T> ptr) => ptr.Target;

    // dereference operator (unary + subbing for *)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T operator +(Pointer<T> ptr) => ptr.Target;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(T* other)
        => Value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Pointer<T> other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
        => obj is Pointer<T> other
           && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Pointer<T> left, Pointer<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Pointer<T> left, Pointer<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => ((nuint) Value).GetHashCode();

    public override string ToString()
        => Value is null ? "null" : $"0x{(ulong) Value:X8}";
}