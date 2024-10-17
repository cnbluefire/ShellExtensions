using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.Helpers
{
    public class LogHelper
    {
        private bool disposeValue;
        private readonly string filePath;
        private System.Collections.Concurrent.BlockingCollection<string> messages;
        private Thread? thread;

        public LogHelper(string filePath)
        {
            this.filePath = filePath;
            messages = new System.Collections.Concurrent.BlockingCollection<string>();
        }

        internal void Dispose()
        {
            if (!disposeValue)
            {
                lock (messages)
                {
                    if (!disposeValue)
                    {
                        disposeValue = true;

                        messages.CompleteAdding();

                        if (thread != null)
                        {
                            thread.Join();
                        }
                    }
                }
            }
        }

        public void LogInfo(string? message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            LogCore(message);
        }

        public void LogError(Exception ex, [CallerMemberName] string? callerName = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            LogCore(BuildMessage(ex, null, callerName, callerFilePath, callerLineNumber));
        }

        public void LogError(string? message, Exception ex, [CallerMemberName] string? callerName = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            LogCore(BuildMessage(ex, message, callerName, callerFilePath, callerLineNumber));
        }

        [Conditional("DEBUG")]
        internal void Debug(string? message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            LogCore(message);
        }

        [Conditional("DEBUG")]
        internal void DebugError(Exception ex, [CallerMemberName] string? callerName = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            LogCore(BuildMessage(ex, null, callerName, callerFilePath, callerLineNumber));
        }

        [Conditional("DEBUG")]
        internal void DebugError(string? message, Exception ex, [CallerMemberName] string? callerName = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            LogCore(BuildMessage(ex, message, callerName, callerFilePath, callerLineNumber));
        }

        private static string? BuildMessage(Exception? exception, string? message, string? callerName, string? callerFilePath, int callerLineNumber)
        {
            var sourceFileName = "";

            if (!string.IsNullOrEmpty(callerFilePath))
            {
                sourceFileName = System.IO.Path.GetFileName(callerFilePath);
            }

            var sb = new StringBuilder(200);

            if (!string.IsNullOrEmpty(sourceFileName))
            {
                sb.Append(sourceFileName).Append(' ');
            }
            if (!string.IsNullOrEmpty(callerName))
            {
                sb.Append('(')
                    .Append(callerName)
                    .Append(')')
                    .Append(' ');
            }
            if (callerLineNumber > 0 && !string.IsNullOrEmpty(sourceFileName))
            {
                sb.Append("#")
                    .Append(callerLineNumber)
                    .Append(' ');
            }

            if (!string.IsNullOrEmpty(message))
            {
                sb.Append("Message: ")
                    .Append(message);
            }

            if (exception != null)
            {
                sb.AppendLine().Append(exception.ToString());
            }

            sb.AppendLine();

            return sb.ToString();
        }

        private void LogCore(string? message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (disposeValue) return;

            messages.Add($"[{DateTime.Now:u}] {message}");

            if (thread == null)
            {
                lock (messages)
                {
                    if (thread == null)
                    {
                        thread = new Thread(static instance =>
                        {
                            if (instance is LogHelper logHelper)
                            {
                                foreach (var msg in logHelper.messages.GetConsumingEnumerable())
                                {
                                    AppendLineCore(logHelper.filePath, msg);
                                }
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start(this);
                    }
                }
            }
        }

        private static void AppendLineCore(string filePath, string content)
        {
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directory)) return;
            if (string.IsNullOrEmpty(fileName)) fileName = "Log.txt";

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            try
            {
                File.AppendAllText(filePath, content, Encoding.UTF8);
            }
            catch { }
        }


        private static LogHelper? instance;
        private static object locker = new object();

        public static LogHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShellExtensions", "Log.txt");
                            instance = new LogHelper(filePath);
                        }
                    }
                }
                return instance;
            }
        }
    }
}