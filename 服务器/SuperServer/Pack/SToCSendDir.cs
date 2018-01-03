using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    class SToCSendDir
    {
        public Pack Packet = new Pack();
        private UInt32 PacketLength_;

        Byte ProtrolNum_ = 0xA1;

        Byte AnswerType_;

        public int PacketLength
        {
            get { return Convert.ToInt32(PacketLength_); }
            set { PacketLength_ = (UInt32)value; }
        }
        
        private Byte[] Diretory_;
        public Byte[] Diretory
        {
            get { return Diretory_; }
            set { Diretory_ = value; }
        }

        public void PackToClient(int length)
        {
            Packet.buf_ = new byte[length];
            AnswerType_ = (Byte)SToCAnswerType.FileListAnswer;
            Packet.PackByte(ProtrolNum_);
            Packet.PackByte(AnswerType_);
            Packet.PackUint32(PacketLength_);
            Packet.PackBytes(Diretory_);
        }
    }
}