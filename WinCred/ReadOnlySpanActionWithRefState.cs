﻿namespace WinCred;

public delegate void ReadOnlySpanActionWithRefState<T, TRefState>(
    ReadOnlySpan<T> span,
    ref TRefState state
);