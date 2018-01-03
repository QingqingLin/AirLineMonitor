using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperServer
{
    class HandlePacket
    {
        PacketProperties PacketProperties;
        static byte[] _packetDateTimeTicksBuff = new byte[8];
        static byte[] _realTimePacketBuff = new byte[2008];
        static IPEndPoint RealTimeEP;

        public HandlePacket(IPEndPoint RealTimeEP, byte[] buf, int len)
        {
            HandlePacket.RealTimeEP = RealTimeEP;
            PacketProperties = new PacketProperties(len);
            Unpack(buf, len);
        }

        unsafe private void Unpack(byte[] buf, int len)
        {
            try
            {
                byte protocol = 0;
                uint version = 0;
                uint ipSourceAddress = 0;
                uint ipDestinationAddress = 0;
                int sourcePort = 0;
                int destinationPort = 0;
                IPAddress ip;

                Array.Copy(buf, PacketProperties.ReceiveBuf, len);
                PacketProperties.BufLength = len;
                fixed (byte* FixedBuf = buf)
                {
                    IPHeader* head = (IPHeader*)FixedBuf;
                    PacketProperties.IPHeaderLength = (uint)((head->versionAndLength & 0x0f) << 2);
                    protocol = head->protocol;
                    version = (uint)((head->versionAndLength & 0xf0) >> 4);
                    PacketProperties.Version = version.ToString();

                    if (protocol == 17 && version == 4)
                    {
                        ipSourceAddress = head->sourceAddress;
                        ipDestinationAddress = head->destinationAdress;
                        ip = new IPAddress(ipSourceAddress);
                        PacketProperties.Source.IP = ip;
                        ip = new IPAddress(ipDestinationAddress);
                        PacketProperties.Dest.IP = ip;
                        sourcePort = buf[PacketProperties.IPHeaderLength] * 256 + buf[PacketProperties.IPHeaderLength + 1];
                        destinationPort = buf[PacketProperties.IPHeaderLength + 2] * 256 + buf[PacketProperties.IPHeaderLength + 3];
                        PacketProperties.Source.Port = sourcePort;
                        PacketProperties.Dest.Port = destinationPort;
                        PacketProperties.PacketLength = (uint)len;
                        if (PacketProperties.PacketLength > PacketProperties.IPHeaderLength + 40)
                        {
                            PacketProperties.MessageLength = PacketProperties.PacketLength - PacketProperties.IPHeaderLength;
                            Array.Copy(buf, (int)PacketProperties.IPHeaderLength, PacketProperties.MessageBuffer, 0, (int)PacketProperties.MessageLength);
                            DataFilterByIPAndPort(PacketProperties);
                        }
                    }

                    else if (protocol == 41 && version == 4)
                    {
                        ipSourceAddress = head->sourceAddress;
                        ipDestinationAddress = head->destinationAdress;
                        ip = new IPAddress(ipSourceAddress);
                        PacketProperties.Source.IP = ip;
                        ip = new IPAddress(ipDestinationAddress);
                        PacketProperties.Dest.IP = ip;
                        sourcePort = buf[PacketProperties.IPHeaderLength] * 256 + buf[PacketProperties.IPHeaderLength + 1];
                        destinationPort = buf[PacketProperties.IPHeaderLength + 2] * 256 + buf[PacketProperties.IPHeaderLength + 3];
                        PacketProperties.Source.Port = sourcePort;
                        PacketProperties.Dest.Port = destinationPort;
                        PacketProperties.PacketLength = (uint)len;
                        if (PacketProperties.PacketLength > PacketProperties.IPHeaderLength + 40)
                        {
                            PacketProperties.MessageLength = PacketProperties.PacketLength - PacketProperties.IPHeaderLength - 40;
                            Array.Copy(buf, (int)PacketProperties.IPHeaderLength + 40, PacketProperties.MessageBuffer, 0, (int)PacketProperties.MessageLength);
                            DataFilterByIPAndPort(PacketProperties);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
            }
        }

        private void DataFilterByIPAndPort(PacketProperties Properties)
        { 
            if (MainForm.filterInfo.Dest.Contains(Properties.Dest))
            {
                if (MainForm.filterInfo.Source.Contains(Properties.Source))
                {
                    CatchData.TemporaryStorage.Enqueue(Properties);
                }
            }
        }

        public static void SendRealTimePacket(byte[] packetBuff, int packetLen, DateTime packetDateTime)
        {
            if (packetLen < 3000)
            {
                _packetDateTimeTicksBuff = BitConverter.GetBytes(packetDateTime.Ticks);
                Array.Copy(_packetDateTimeTicksBuff, 0, _realTimePacketBuff, 0, _packetDateTimeTicksBuff.Length);
                Array.Copy(packetBuff, 0, _realTimePacketBuff, _packetDateTimeTicksBuff.Length, packetLen);
                Socket socket = MainForm.CommunicationSocketDic[SocketTypes.ServerToClientData];
                int sendNum = socket.SendTo(_realTimePacketBuff,0, packetLen + _packetDateTimeTicksBuff.Length, SocketFlags.None, RealTimeEP);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IPHeader
        {
            [FieldOffset(0)]
            public byte versionAndLength;
            [FieldOffset(1)]
            public byte typeOfServices;
            [FieldOffset(2)]
            public ushort totalLength;
            [FieldOffset(4)]
            public ushort identifier;
            [FieldOffset(6)]
            public ushort flagsAndOffset;
            [FieldOffset(8)]
            public byte timeToLive;
            [FieldOffset(9)]
            public byte protocol;
            [FieldOffset(10)]
            public ushort checksum;
            [FieldOffset(12)]
            public uint sourceAddress;
            [FieldOffset(16)]
            public uint destinationAdress;
        }
    }
}
