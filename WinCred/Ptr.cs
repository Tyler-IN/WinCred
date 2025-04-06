#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace WinCred;

[PublicAPI]
[ExcludeFromCodeCoverage]
public unsafe struct Ptr<T> : IEquatable<Ptr<T>>
{
    public T* Value;

    public readonly ref T Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<T>(Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ptr(T* value) => Value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(Ptr<T> ptr) => ptr.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr<T>(T* pointer) => new(pointer);

    // dereference operator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Ptr<T> ptr) => ptr.Target;

    // dereference operator (unary + subbing for *)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T operator +(Ptr<T> ptr) => ptr.Target;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(T* other)
        => Value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Ptr<T> other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
        => obj is Ptr<T> other
           && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ptr<T> left, Ptr<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ptr<T> left, Ptr<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => ((nuint) Value).GetHashCode();

    public override string ToString()
        => Value is null ? "null" : $"0x{(ulong) Value:X8}";
}