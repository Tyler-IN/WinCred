namespace WinCred;

public delegate void ReadOnlySpanAction<T>(ReadOnlySpan<T> span);
public delegate void ReadOnlySpanActionWithRefState<T, TRefState>(ReadOnlySpan<T> span, ref TRefState state);