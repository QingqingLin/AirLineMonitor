using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperServer
{
    public class PacketProperties
    {
        private byte[] _ReceiveBuf;
        private int _BufLength;
        private DeviceInfo source;
        private DeviceInfo dest;
        private string version;
        private uint packetLength;
        private uint messageLength;
        private uint ipHeaderLength;
        private byte[] messageBuffer = null;
        private DateTime _captureTime;

        public PacketProperties(int receiveBufferLength)
        {
            source = new DeviceInfo();
            dest = new DeviceInfo();
            _captureTime = DateTime.Now;
            _ReceiveBuf = new byte[receiveBufferLength];
            messageBuffer = new byte[receiveBufferLength];
        }

        public byte[] ReceiveBuf
        {
            get { return _ReceiveBuf; }
            set { _ReceiveBuf = value; }
        }

        public int BufLength
        {
            get { return _BufLength; }
            set { _BufLength = value; }
        }

        public DeviceInfo Dest
        {
            get { return dest; }
            set { dest = value; }
        }

        public DeviceInfo Source
        {
            get { return source; }
            set { source = value; }
        }

        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        public uint PacketLength
        {
            get { return packetLength; }
            set { packetLength = value; }
        }

        public uint MessageLength
        {
            get { return messageLength; }
            set { messageLength = value; }
        }


        public uint IPHeaderLength
        {
            get { return ipHeaderLength; }
            set { ipHeaderLength = value; }
        }

        public byte[] MessageBuffer
        {
            get { return messageBuffer; }
            set { messageBuffer = value; }
        }

        public DateTime CaptureTime
        {
            get { return _captureTime; }
        }
    }
}
