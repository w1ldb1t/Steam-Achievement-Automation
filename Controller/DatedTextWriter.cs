using System;
using System.IO;
using System.Text;

namespace Controller
{
    internal class DatedTextWriter : TextWriter
    {
        private TextWriter TextWriter;
        private const string kFormat = "[{0}] {1}";

        public DatedTextWriter()
        {
            this.TextWriter = Console.Out;
        }
        public override Encoding Encoding
        {
            get { return new ASCIIEncoding(); }
        }
        public override void WriteLine(string message)
        {
            TextWriter.WriteLine(String.Format(kFormat, DateTime.Now, message));
        }
        public override void Write(string message)
        {
            TextWriter.Write(String.Format(kFormat, DateTime.Now, message));
        }
    }
}
