using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BIMformative.Infrastructure.Logging
{
    public sealed class FileLogger : IAppLogger
    {
        private static readonly object SyncRoot = new object();

        private readonly string _logFolder;
        private readonly string _logFilePath;
        private readonly LogLevel _minimumLevel;

        public FileLogger(string appName = "BIMformative", LogLevel minimumLevel = LogLevel.Info)
        {
            if (string.IsNullOrWhiteSpace(appName))
                throw new ArgumentException("App name cannot be empty.", nameof(appName));

            _minimumLevel = minimumLevel;

            _logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "Logs");

            Directory.CreateDirectory(_logFolder);

            _logFilePath = Path.Combine(_logFolder, "bimformative.log");
            
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Error(string message, Exception exception)
        {
            Log(LogLevel.Error, message, exception);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Log(LogLevel level, string message)
        {
            if (level < _minimumLevel)
                return;

            WriteLine(level, message, null);
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message)
        {
            throw new NotImplementedException();
        }

        private void WriteLine(LogLevel level, string message, Exception exception)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "(empty message)";

            var sb = new StringBuilder();

            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(" [");
            sb.Append(level.ToString().ToUpperInvariant());
            sb.Append("] ");
            sb.Append(message);

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append("Exception: ");
                sb.Append(exception.GetType().FullName);
                sb.AppendLine();
                sb.Append("Message: ");
                sb.Append(exception.Message);
                sb.AppendLine();
                sb.Append("StackTrace: ");
                sb.Append(exception.StackTrace);
            }

            var line = sb.ToString() + Environment.NewLine;

            try
            {
                System.Diagnostics.Debug.WriteLine(line);

                lock (SyncRoot)
                {
                    File.AppendAllText(_logFilePath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Never let logging crash the app.
            }
        }
    }
}
