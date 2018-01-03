using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    /// <summary>
    /// 协议头部
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ProtocolHeader
    {
        /// <summary>
        /// 协议标识A1
        /// </summary>
        [FieldOffset(0)]
        public byte identifier;
        /// <summary>
        /// 操作类型
        /// </summary>
        [FieldOffset(1)]
        public CToSCommandType cTosCommandType;
        ///// <summary>
        ///// 申请/响应
        ///// </summary>
        [FieldOffset(2)]
        public UInt16 PacketLength;

    }

    class ServerToClientSend
    {
        public bool sendSeverCmd(SocketTypes conName, byte[] cmd, int lenth)
        {
            try
            {
                int lengthOfNotSend = lenth;
                Socket socket = null;
                while (lengthOfNotSend > 0)
                {
                    MainForm.CommunicationSocketDic.TryGetValue(conName, out socket);
                    int len = socket.Send(cmd, 0, lenth, SocketFlags.None);
                    lengthOfNotSend -= len;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
