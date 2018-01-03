using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    public enum FileRequestMode
    {
        Send = 0,
        Receive
    }

    class FileProtocol
    {
        private readonly FileRequestMode mode;
        private readonly int port;
        private readonly string fileName;
        private readonly double length;

        public FileProtocol(FileRequestMode mode, int port, string fileName, double length)
        {
            this.mode = mode;
            this.port = port;
            this.length = length;
            this.fileName = fileName;
        }
        public FileRequestMode Mode { get { return mode; } }

        public int Port
        {
            get
            {
                return port;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public double Length
        {
            get
            {
                return length;
            }
        }


        public override string ToString()
        {
            return string.Format("<protocol><file name=\"{0}\" mode=\"{1}\" port=\"{2}\" length=\"{3}\"/></protocol>", fileName, mode, port, length);
        }
    }
}
