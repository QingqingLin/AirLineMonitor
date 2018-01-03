using System;

namespace SuperServer
{
    class Unpack
    {
        public ProtocolHeader Header; 
        int byteFlag_;
        int bitFlag_;
        byte[] buf_;

        public void Reset(byte[] buf)
        {
            buf_ = buf;
            Header = new ProtocolHeader();
            byteFlag_ = 0;
            bitFlag_ = 0;
        }

        public Byte GetByte()
        {
            Byte value = buf_[byteFlag_];
            byteFlag_++;
            return value;
        }

        public UInt16 GetUint16()
        {
            UInt16 value = (UInt16)(buf_[byteFlag_ + 1] << 8);
            value |= buf_[byteFlag_];
            byteFlag_ += 2;
            return value;
        }

        public bool GetBit()
        {
            bool result = ((buf_[byteFlag_] >> (7 - bitFlag_++)) & 1) == 1;
            if (bitFlag_ == 8)
            {
                Skip();
            }
            return result;
        }

        public void Skip()
        {
            if (bitFlag_ != 0)
            {
                byteFlag_++;
                bitFlag_ = 0;
            }
        }

        public Byte[] GetBytes(UInt16 Length)
        {
            byte[] Bytes = new byte[Length];
            Array.Copy(buf_, byteFlag_, Bytes, 0, Length);
            return Bytes;
        }
    }
}
