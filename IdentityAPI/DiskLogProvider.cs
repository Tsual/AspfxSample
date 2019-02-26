using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityAPI
{
    public class DiskLogProvider : ILoggerProvider
    {
        internal TextWriter textWriter;
        private readonly Timer timer;
        public DiskLogProvider()
        {
            var log_dir = new DirectoryInfo("log");
            if (!log_dir.Exists) log_dir.Create();
            string log_name = NewName();
            while (File.Exists(NewPath(log_name)))
                log_name = NewName();
            textWriter = new FileInfo(NewPath(log_name)).CreateText();
            timer = new Timer(t => { textWriter.Flush(); }, null, 0, 500);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DiskLogger(this, categoryName);
        }

        private string NewName()
        {
            return (DateTime.Now.ToShortDateString() + "_" + Guid.NewGuid().ToString()).Replace('\\', '-').Replace('/', '-');
        }

        private string NewPath(string name)
        {
            return "log/" + name + ".log";
        }

        public void Dispose()
        {
            return;
        }


        public class DiskLogger : ILogger
        {
            private readonly DiskLogProvider p;
            private readonly string categoryName;
            public DiskLogger(DiskLogProvider p, string categoryName)
            {
                this.p = p;
                this.categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                p.textWriter.WriteLine(
                    "[" + DateTime.Now.ToString() + "] [" + eventId + "] (" + categoryName + ") " + logLevel + " " + formatter?.Invoke(state, exception)
                    );
            }
        }
    }


}
