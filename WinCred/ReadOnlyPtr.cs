#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace WinCred;

[PublicAPI]
[ExcludeFromCodeCoverage]
public unsafe readonly struct ReadOnlyPtr<T> : IEquatable<Ptr<T>>
{
    private readonly T* _value;

    public ref readonly T Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<T>(_value);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyPtr(T* value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyPtr(Ptr<T> value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyPtr<T>(T* pointer) => new(pointer);

    // dereference operator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(ReadOnlyPtr<T> ptr) => ptr.Target;

    // dereference operator (unary + subbing for *)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T operator +(ReadOnlyPtr<T> ptr) => ptr.Target;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyPtr<T>(Ptr<T> pointer) => new(pointer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(T* other)
        => _value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Ptr<T> other)
        => _value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlyPtr<T> other)
        => _value == other._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => (obj is ReadOnlyPtr<T> other
            && Equals(other))
           || (obj is Ptr<T> otherPtr
               && Equals(otherPtr));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ReadOnlyPtr<T> left, ReadOnlyPtr<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ReadOnlyPtr<T> left, ReadOnlyPtr<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ReadOnlyPtr<T> left, Ptr<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ReadOnlyPtr<T> left, Ptr<T> right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => ((nuint) _value).GetHashCode();

    public override string ToString()
        => _value is null ? "null" : $"0x{(ulong) _value:X8}";
}