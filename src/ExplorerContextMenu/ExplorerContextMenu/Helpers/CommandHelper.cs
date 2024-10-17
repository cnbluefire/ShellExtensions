using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                        UseShellExecute = true
                    };
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    psi.ArgumentList.Add(token);
                }
            }

            return psi;
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
    }
}
