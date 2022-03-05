using System;
using Xunit.Abstractions;

namespace TheUniversalCity.RedisClient.Test
{
    public class OutputLog : IDisposable
    {
        public OutputLog(IMessageSink diagnosticMessageSink)
        {
            DiagnosticMessageSink = diagnosticMessageSink;
        }

        public IMessageSink DiagnosticMessageSink { get; }

        public void Dispose()
        {
            
        }
    }
}
