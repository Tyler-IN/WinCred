#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace WinCred;

[PublicAPI]
[ExcludeFromCodeCoverage]
public readonly unsafe struct ReadOnlyPointer<T> : IEquatable<Pointer<T>>
{
    private readonly T* _value;

    public ref readonly T Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<T>(_value);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyPointer(T* value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyPointer(Pointer<T> value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyPointer<T>(T* pointer) => new(pointer);

    // dereference operator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(ReadOnlyPointer<T> ptr) => ptr.Target;

    // dereference operator (unary + subbing for *)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T operator +(ReadOnlyPointer<T> ptr) => ptr.Target;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyPointer<T>(Pointer<T> pointer) => new(pointer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(T* other)
        => _value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Pointer<T> other)
        => _value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlyPointer<T> other)
        => _value == other._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => (obj is ReadOnlyPointer<T> other
            && Equals(other))
           || (obj is Pointer<T> otherPtr
               && Equals(otherPtr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ReadOnlyPointer<T> left, Pointer<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ReadOnlyPointer<T> left, Pointer<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => ((nuint) _value).GetHashCode();

    public override string ToString()
        => _value is null ? "null" : $"0x{(ulong) _value:X8}";
}