using BIMformative.Core.Models.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIMformative.Core.Interfaces
{
    public interface IAppLogger
    {
        void Log(LogLevel level, string message);
        void Log(LogLevel level, string message, Exception exception);

        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Error(string message, Exception exception);
    }
}
