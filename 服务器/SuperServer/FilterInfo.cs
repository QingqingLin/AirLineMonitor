using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer
{
    public enum PcapLengthUnit : Byte
    {
        /// <summary>
        /// MB
        /// </summary>
        MB = 1,
        /// <summary>
        /// GB
        /// </summary>
        GB = 2
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// IP
        /// </summary>
        public IPAddress IP;
        /// <summary>
        /// Port
        /// </summary>
        public int Port;

        public override bool Equals(object obj)
        {
            DeviceInfo di = (DeviceInfo)obj;
            bool isEquals = false;
            if (di.IP.ToString() == this.IP.ToString() && di.Port == this.Port)
            {
                isEquals = true;
            }
            return isEquals;
        }
    }

    public class FilterInfo
    {
        private double PcapLengthNum_;
        /// <summary>
        /// Pcap文件保存大小
        /// </summary>
        public double PcapLengthNum
        {
            get { return PcapLengthNum_; }
            set { PcapLengthNum_ = value; }
        }

        private List<DeviceInfo> Source_;

        /// <summary>
        /// 用户选择的源设备的IP和端口的集合
        /// </summary>
        public List<DeviceInfo> Source
        {
            get { return Source_; }
            set { Source_ = value; }
        }

        private List<DeviceInfo> Dest_;
        /// <summary>
        /// 用户选择的目标设备的IP和端口的集合
        /// </summary>
        public List<DeviceInfo> Dest
        {
            get { return Dest_; }
            set { Dest_ = value; }
        }

        private UInt16 MaxVOBCToZC_;
        /// <summary>
        /// VOBC到ZC端的告警门限
        /// </summary>
        public UInt16 MaxVOBCToZC
        {
            get { return MaxVOBCToZC_; }
            set { MaxVOBCToZC_ = value; }
        }

        private UInt16 MaxZCToVOBC_;
        /// <summary>
        /// ZC到VOBC端的告警门限
        /// </summary>
        public UInt16 MaxZCToVOBC
        {
            get { return MaxZCToVOBC_; }
            set { MaxZCToVOBC_ = value; }
        }

        private UInt16 MaxVOBCToCI_;
        /// <summary>
        /// VOBC到CI端的告警门限
        /// </summary>
        public UInt16 MaxVOBCToCI
        {
            get { return MaxVOBCToCI_; }
            set { MaxVOBCToCI_ = value; }
        }

        private UInt16 MaxCIToVOBC_;
        /// <summary>
        /// CI到VOBC端的告警门限
        /// </summary>
        public UInt16 MaxCIToVOBC
        {
            get { return MaxCIToVOBC_; }
            set { MaxCIToVOBC_ = value; }
        }

        private UInt16 MaxVOBCToATS_;
        /// <summary>
        /// VOBC到ATS端的告警门限
        /// </summary>
        public UInt16 MaxVOBCToATS
        {
            get { return MaxVOBCToATS_; }
            set { MaxVOBCToATS_ = value; }
        }

        private UInt16 MaxATSToVOBC_;
        /// <summary>
        /// ATS到VOBC端的告警门限
        /// </summary>
        public UInt16 MaxATSToVOBC
        {
            get { return MaxATSToVOBC_; }
            set { MaxATSToVOBC_ = value; }
        }

        private Byte DeleteCycle_;
        /// <summary>
        /// 数据删除周期
        /// </summary>
        public Byte DeleteCycle
        {
            get { return DeleteCycle_; }
            set { DeleteCycle_ = value; }
        }

        public FilterInfo()
        {
            Source_ = new List<DeviceInfo>();
            Dest_ = new List<DeviceInfo>();
        }
    }
}
