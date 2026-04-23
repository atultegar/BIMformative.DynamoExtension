using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIMformative.Infrastructure.Logging
{
    public sealed class NullLogger : IAppLogger
    {
        public void Debug(string message)
        {
            
        }

        public void Error(string message)
        {
            
        }

        public void Error(string message, Exception exception)
        {
            
        }

        public void Info(string message)
        {
            
        }

        public void Log(LogLevel level, string message)
        {
           
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            
        }

        public void Warning(string message)
        {
            
        }
    }
}
