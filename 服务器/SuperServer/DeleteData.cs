using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace SuperServer
{
    class DeleteData
    {
        TimeSpan timeSpan;
        ushort DeleteDataInterval;
        string Path = Application.StartupPath;

        public DeleteData(ushort deleteDataInterval)
        {
            this.DeleteDataInterval = deleteDataInterval;
            timeSpan = new TimeSpan(deleteDataInterval, 0, 0, 0);
        }

        public void DeleteOverdueData()
        {
            DateTime dtNow = DateTime.Now;
            DateTime delTime = dtNow - timeSpan;
            try
            {
                string cfgDirPath = Path + @"\pcap";
                DirectoryInfo DirYear = new DirectoryInfo(cfgDirPath);
                foreach (DirectoryInfo YearFolder in DirYear.GetDirectories())
                {
                    if (Convert.ToInt16(YearFolder.Name) < delTime.Year)
                    {
                        DeleteFolder(YearFolder.FullName);
                    }
                    else if (Convert.ToInt16(YearFolder.Name) == delTime.Year)
                    {
                        foreach (DirectoryInfo MonthFolder in YearFolder.GetDirectories())
                        {
                            if (Convert.ToInt16(MonthFolder.Name) < delTime.Month)
                            {
                                DeleteFolder(MonthFolder.FullName);
                            }
                            else if (Convert.ToInt16(MonthFolder.Name) == delTime.Month)
                            {
                                foreach (DirectoryInfo DayFolder in MonthFolder.GetDirectories())
                                {
                                    if (Convert.ToInt16(DayFolder.Name) < delTime.Day)
                                    {
                                        DeleteFolder(DayFolder.FullName);
                                    }
                                    else if (Convert.ToInt16(DayFolder.Name) == delTime.Day)
                                    {
                                        DeleteFileInDayFolder(DayFolder, delTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                String fileLogName = System.IO.Path.Combine(System.IO.Path.Combine(Application.StartupPath, "ErrorLogData"), "Monitor" + DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.CurrentCulture) + ".log");
                if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileLogName)))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileLogName));
                }
                System.IO.StreamWriter log = new System.IO.StreamWriter(new System.IO.FileStream(fileLogName, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite));
                System.Text.StringBuilder buff = new System.Text.StringBuilder();

                buff.Append(String.Format(CultureInfo.CurrentCulture, "-----------------------------start({0})------------------------------------\n", DateTime.Now.ToString("o", System.Globalization.CultureInfo.CurrentCulture)));

                buff.Append(e.Message + "\n");

                buff.Append("---------------------------------------End---------------------------------------\n");

                log.WriteLine(buff.ToString());
                log.Flush();
                log.Close();
            }
        }

        private void DeleteFileInDayFolder(DirectoryInfo dayFolder, DateTime delTime)
        {
            foreach (var PcapFile in dayFolder.GetDirectories())
            {
                DateTime FileCreateTime = PcapFile.CreationTime;
                if (FileCreateTime < delTime)
                {
                    DeleteFile(PcapFile.FullName);
                }
            }
        }

        private void DeleteFolder(string FolderPath)
        {
            DirectoryInfo Folder = new DirectoryInfo(FolderPath);
            Folder.Delete(true);
        }

        private void DeleteFile(string FilePath)
        {
            File.Delete(FilePath);
        }
    }
}
