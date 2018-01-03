using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    class SToCConnectAnswer
    {
        public Pack Packet = new Pack();
        public int PacketLength;

        Byte ProtrolNum_ = 0xA1;

        Byte AnswerType_;

        private Byte RealTimeConnectionFlag_;
        public Byte RealTImeConnectionFlag
        {
            get { return RealTimeConnectionFlag_; }
            set { RealTimeConnectionFlag_ = value; }
        }

        private Byte CatchSocketFlag_;
        public Byte CatchSocketFlag
        {
            get { return CatchSocketFlag_; }
            set { CatchSocketFlag_ = value; }
        }

        public SToCConnectAnswer()
        {
            AnswerType_ = (Byte)SToCAnswerType.ConnectAndFilterInfoAnswer;
            Packet.PackByte(ProtrolNum_);
            Packet.PackByte(AnswerType_);
            PacketLength = 4;
        }
    }
}
