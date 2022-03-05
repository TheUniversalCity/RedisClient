using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TheUniversalCity.RedisClient.Test
{
    class XUnitTextWriter : TextWriter
    {
        private readonly ITestOutputHelper output;
        private readonly IMessageSink diagnosticMessageSink;

        public XUnitTextWriter(ITestOutputHelper output, IMessageSink diagnosticMessageSink)
        {
            this.output = output;
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void WriteLine(string message)
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
            output.WriteLine(message);
        }

        public override void WriteLine(string format, params object[] args)
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage(format, args));
            output.WriteLine(format, args);
        }

        public override void Write(char value)
        {
            throw new NotSupportedException("This text writer only supports WriteLine(string) and WriteLine(string, params object[]).");
        }
    }
}
