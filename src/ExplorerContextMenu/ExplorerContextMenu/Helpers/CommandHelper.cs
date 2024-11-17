using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Helpers
{
    internal static class CommandHelper
    {
        internal static ProcessStartInfo? CreateProcessStartInfo(string command)
        {
            ProcessStartInfo? psi = null;

            foreach (var token in CommandLineStringSplitter.Instance.Split(command))
            {
                if (psi == null)
                {
                    if (string.IsNullOrEmpty(token)) return null;
                    psi = new ProcessStartInfo(token)
                    {
                        UseShellExecute = true,
                    };
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    psi.ArgumentList.Add(token);
                }
            }

            return psi;
        }

        internal static bool TryParseCommand(string command, [NotNullWhen(true)] out string? fileName, out string? parameters)
        {
            fileName = null;
            parameters = null;
            if (string.IsNullOrEmpty(command)) return false;

            var arguments = new ValueStringBuilder(stackalloc char[256]);

            foreach (var token in CommandLineStringSplitter.Instance.Split(command))
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    if (string.IsNullOrEmpty(token)) return false;
                    fileName = token;
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    PasteArguments.AppendArgument(ref arguments, token);
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (arguments.Length > 0)
                {
                    parameters = arguments.ToString();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// <see href="https://github.com/dotnet/command-line-api/blob/v2.0.0-beta3.22114.1/src/System.CommandLine/Parsing/CommandLineStringSplitter.cs"/>
        /// </summary>
        internal class CommandLineStringSplitter
        {
            public static readonly CommandLineStringSplitter Instance = new();

            private CommandLineStringSplitter()
            {
            }

            private enum Boundary
            {
                TokenStart,
                WordEnd,
                QuoteStart,
                QuoteEnd
            }

            public IEnumerable<string> Split(string commandLine)
            {
                var memory = commandLine.AsMemory();

                var startTokenIndex = 0;

                var pos = 0;

                var seeking = Boundary.TokenStart;
                var seekingQuote = Boundary.QuoteStart;

                while (pos < memory.Length)
                {
                    var c = memory.Span[pos];

                    if (char.IsWhiteSpace(c))
                    {
                        if (seekingQuote == Boundary.QuoteStart)
                        {
                            switch (seeking)
                            {
                                case Boundary.WordEnd:
                                    yield return CurrentToken();
                                    startTokenIndex = pos;
                                    seeking = Boundary.TokenStart;
                                    break;

                                case Boundary.TokenStart:
                                    startTokenIndex = pos;
                                    break;
                            }
                        }
                    }
                    else if (c == '\"')
                    {
                        if (seeking == Boundary.TokenStart)
                        {
                            switch (seekingQuote)
                            {
                                case Boundary.QuoteEnd:
                                    yield return CurrentToken();
                                    startTokenIndex = pos;
                                    seekingQuote = Boundary.QuoteStart;
                                    break;

                                case Boundary.QuoteStart:
                                    startTokenIndex = pos + 1;
                                    seekingQuote = Boundary.QuoteEnd;
                                    break;
                            }
                        }
                        else
                        {
                            switch (seekingQuote)
                            {
                                case Boundary.QuoteEnd:
                                    seekingQuote = Boundary.QuoteStart;
                                    break;

                                case Boundary.QuoteStart:
                                    seekingQuote = Boundary.QuoteEnd;
                                    break;
                            }
                        }
                    }
                    else if (seeking == Boundary.TokenStart && seekingQuote == Boundary.QuoteStart)
                    {
                        seeking = Boundary.WordEnd;
                        startTokenIndex = pos;
                    }

                    Advance();

                    if (IsAtEndOfInput())
                    {
                        switch (seeking)
                        {
                            case Boundary.TokenStart:
                                break;
                            default:
                                yield return CurrentToken();
                                break;
                        }
                    }
                }

                void Advance() => pos++;

                string CurrentToken()
                {
                    return memory.Slice(startTokenIndex, IndexOfEndOfToken()).ToString().Replace("\"", "");
                }

                int IndexOfEndOfToken() => pos - startTokenIndex;

                bool IsAtEndOfInput() => pos == memory.Length;
            }
        }

        /// <summary>
        /// <see href="https://github.com/dotnet/runtime/blob/v9.0.0/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs" />
        /// </summary>
        internal static partial class PasteArguments
        {
            internal static void AppendArgument(ref ValueStringBuilder stringBuilder, string argument)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append(' ');
                }

                // Parsing rules for non-argv[0] arguments:
                //   - Backslash is a normal character except followed by a quote.
                //   - 2N backslashes followed by a quote ==> N literal backslashes followed by unescaped quote
                //   - 2N+1 backslashes followed by a quote ==> N literal backslashes followed by a literal quote
                //   - Parsing stops at first whitespace outside of quoted region.
                //   - (post 2008 rule): A closing quote followed by another quote ==> literal quote, and parsing remains in quoting mode.
                if (argument.Length != 0 && ContainsNoWhitespaceOrQuotes(argument))
                {
                    // Simple case - no quoting or changes needed.
                    stringBuilder.Append(argument);
                }
                else
                {
                    stringBuilder.Append(Quote);
                    int idx = 0;
                    while (idx < argument.Length)
                    {
                        char c = argument[idx++];
                        if (c == Backslash)
                        {
                            int numBackSlash = 1;
                            while (idx < argument.Length && argument[idx] == Backslash)
                            {
                                idx++;
                                numBackSlash++;
                            }

                            if (idx == argument.Length)
                            {
                                // We'll emit an end quote after this so must double the number of backslashes.
                                stringBuilder.Append(Backslash, numBackSlash * 2);
                            }
                            else if (argument[idx] == Quote)
                            {
                                // Backslashes will be followed by a quote. Must double the number of backslashes.
                                stringBuilder.Append(Backslash, numBackSlash * 2 + 1);
                                stringBuilder.Append(Quote);
                                idx++;
                            }
                            else
                            {
                                // Backslash will not be followed by a quote, so emit as normal characters.
                                stringBuilder.Append(Backslash, numBackSlash);
                            }

                            continue;
                        }

                        if (c == Quote)
                        {
                            // Escape the quote so it appears as a literal. This also guarantees that we won't end up generating a closing quote followed
                            // by another quote (which parses differently pre-2008 vs. post-2008.)
                            stringBuilder.Append(Backslash);
                            stringBuilder.Append(Quote);
                            continue;
                        }

                        stringBuilder.Append(c);
                    }

                    stringBuilder.Append(Quote);
                }
            }

            private static bool ContainsNoWhitespaceOrQuotes(string s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (char.IsWhiteSpace(c) || c == Quote)
                    {
                        return false;
                    }
                }

                return true;
            }

            private const char Quote = '\"';
            private const char Backslash = '\\';
        }

        /// <summary>
        /// <see href="https://github.com/dotnet/runtime/blob/v9.0.0/src/libraries/Common/src/System/Text/ValueStringBuilder.cs" />
        /// </summary>
        internal ref partial struct ValueStringBuilder
        {
            private char[]? _arrayToReturnToPool;
            private Span<char> _chars;
            private int _pos;

            public ValueStringBuilder(Span<char> initialBuffer)
            {
                _arrayToReturnToPool = null;
                _chars = initialBuffer;
                _pos = 0;
            }

            public ValueStringBuilder(int initialCapacity)
            {
                _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
                _chars = _arrayToReturnToPool;
                _pos = 0;
            }

            public int Length
            {
                get => _pos;
                set
                {
                    Debug.Assert(value >= 0);
                    Debug.Assert(value <= _chars.Length);
                    _pos = value;
                }
            }

            public int Capacity => _chars.Length;

            public void EnsureCapacity(int capacity)
            {
                // This is not expected to be called this with negative capacity
                Debug.Assert(capacity >= 0);

                // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
                if ((uint)capacity > (uint)_chars.Length)
                    Grow(capacity - _pos);
            }

            /// <summary>
            /// Get a pinnable reference to the builder.
            /// Does not ensure there is a null char after <see cref="Length"/>
            /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
            /// the explicit method call, and write eg "fixed (char* c = builder)"
            /// </summary>
            public ref char GetPinnableReference()
            {
                return ref MemoryMarshal.GetReference(_chars);
            }

            /// <summary>
            /// Get a pinnable reference to the builder.
            /// </summary>
            /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
            public ref char GetPinnableReference(bool terminate)
            {
                if (terminate)
                {
                    EnsureCapacity(Length + 1);
                    _chars[Length] = '\0';
                }
                return ref MemoryMarshal.GetReference(_chars);
            }

            public ref char this[int index]
            {
                get
                {
                    Debug.Assert(index < _pos);
                    return ref _chars[index];
                }
            }

            public override string ToString()
            {
                string s = _chars.Slice(0, _pos).ToString();
                Dispose();
                return s;
            }

            /// <summary>Returns the underlying storage of the builder.</summary>
            public Span<char> RawChars => _chars;

            /// <summary>
            /// Returns a span around the contents of the builder.
            /// </summary>
            /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
            public ReadOnlySpan<char> AsSpan(bool terminate)
            {
                if (terminate)
                {
                    EnsureCapacity(Length + 1);
                    _chars[Length] = '\0';
                }
                return _chars.Slice(0, _pos);
            }

            public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
            public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
            public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

            public bool TryCopyTo(Span<char> destination, out int charsWritten)
            {
                if (_chars.Slice(0, _pos).TryCopyTo(destination))
                {
                    charsWritten = _pos;
                    Dispose();
                    return true;
                }
                else
                {
                    charsWritten = 0;
                    Dispose();
                    return false;
                }
            }

            public void Insert(int index, char value, int count)
            {
                if (_pos > _chars.Length - count)
                {
                    Grow(count);
                }

                int remaining = _pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
                _chars.Slice(index, count).Fill(value);
                _pos += count;
            }

            public void Insert(int index, string? s)
            {
                if (s == null)
                {
                    return;
                }

                int count = s.Length;

                if (_pos > (_chars.Length - count))
                {
                    Grow(count);
                }

                int remaining = _pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
                s
#if !NET
                .AsSpan()
#endif
                    .CopyTo(_chars.Slice(index));
                _pos += count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Append(char c)
            {
                int pos = _pos;
                Span<char> chars = _chars;
                if ((uint)pos < (uint)chars.Length)
                {
                    chars[pos] = c;
                    _pos = pos + 1;
                }
                else
                {
                    GrowAndAppend(c);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Append(string? s)
            {
                if (s == null)
                {
                    return;
                }

                int pos = _pos;
                if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
                {
                    _chars[pos] = s[0];
                    _pos = pos + 1;
                }
                else
                {
                    AppendSlow(s);
                }
            }

            private void AppendSlow(string s)
            {
                int pos = _pos;
                if (pos > _chars.Length - s.Length)
                {
                    Grow(s.Length);
                }

                s
#if !NET
                .AsSpan()
#endif
                    .CopyTo(_chars.Slice(pos));
                _pos += s.Length;
            }

            public void Append(char c, int count)
            {
                if (_pos > _chars.Length - count)
                {
                    Grow(count);
                }

                Span<char> dst = _chars.Slice(_pos, count);
                for (int i = 0; i < dst.Length; i++)
                {
                    dst[i] = c;
                }
                _pos += count;
            }

            public unsafe void Append(char* value, int length)
            {
                int pos = _pos;
                if (pos > _chars.Length - length)
                {
                    Grow(length);
                }

                Span<char> dst = _chars.Slice(_pos, length);
                for (int i = 0; i < dst.Length; i++)
                {
                    dst[i] = *value++;
                }
                _pos += length;
            }

            public void Append(scoped ReadOnlySpan<char> value)
            {
                int pos = _pos;
                if (pos > _chars.Length - value.Length)
                {
                    Grow(value.Length);
                }

                value.CopyTo(_chars.Slice(_pos));
                _pos += value.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<char> AppendSpan(int length)
            {
                int origPos = _pos;
                if (origPos > _chars.Length - length)
                {
                    Grow(length);
                }

                _pos = origPos + length;
                return _chars.Slice(origPos, length);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void GrowAndAppend(char c)
            {
                Grow(1);
                Append(c);
            }

            /// <summary>
            /// Resize the internal buffer either by doubling current buffer size or
            /// by adding <paramref name="additionalCapacityBeyondPos"/> to
            /// <see cref="_pos"/> whichever is greater.
            /// </summary>
            /// <param name="additionalCapacityBeyondPos">
            /// Number of chars requested beyond current position.
            /// </param>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Grow(int additionalCapacityBeyondPos)
            {
                Debug.Assert(additionalCapacityBeyondPos > 0);
                Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

                const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

                // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
                // to double the size if possible, bounding the doubling to not go beyond the max array length.
                int newCapacity = (int)Math.Max(
                    (uint)(_pos + additionalCapacityBeyondPos),
                    Math.Min((uint)_chars.Length * 2, ArrayMaxLength));

                // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
                // This could also go negative if the actual required length wraps around.
                char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

                _chars.Slice(0, _pos).CopyTo(poolArray);

                char[]? toReturn = _arrayToReturnToPool;
                _chars = _arrayToReturnToPool = poolArray;
                if (toReturn != null)
                {
                    ArrayPool<char>.Shared.Return(toReturn);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                char[]? toReturn = _arrayToReturnToPool;
                this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
                if (toReturn != null)
                {
                    ArrayPool<char>.Shared.Return(toReturn);
                }
            }
        }
    }
}
