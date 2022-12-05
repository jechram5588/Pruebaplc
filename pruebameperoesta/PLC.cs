using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Corping.PLCConnector;
using log4net;
using System.Threading;


namespace pruebameperoesta
{
    public class ConnectionPLC
    {
        private MemoryItem memRead;
        private MemoryItem memWrite;
        private MemoryItem memLevel;
        private ILog Logger = LogManager.GetLogger(typeof(ConnectionPLC));

        public ConnectionPLC(int otro, int nivel,int temp)
        {
            memRead = new MemoryItem();
            memRead.Device = DeviceType.D;
            memRead.Type = DataType.Int;
            memRead.StartAddr = otro;
            memRead.Size = 1;

            memLevel = new MemoryItem();
            memLevel.Device = DeviceType.D;
            memLevel.Type = DataType.Int;
            memLevel.StartAddr = nivel;
            memLevel.Size = 1;

            memWrite = new MemoryItem();
            memWrite.Device = DeviceType.D;
            memWrite.Type = DataType.Int;
            memWrite.StartAddr = temp;
        }

        public object[] connected(PLC plc, int val)
        {
            try
            {
                object[] a = new object[2];
                plc.WriteMemory(memWrite, val);
                Thread.Sleep(200);
                object obj = plc.ReadMemory(memRead);
                if (obj != null)
                {
                    if (Information.IsNumeric(obj))
                        a[0] = obj;
                    else
                        a[0] = 0;
                }
                else
                    a[0] = 0;
                Thread.Sleep(200);
                object obj2 = plc.ReadMemory(memLevel);
                if (obj2 != null)
                {
                    if (Information.IsNumeric(obj2))
                        a[1] = obj2;
                    else
                        a[1] = 0;
                }
                else
                    a[1] = 0;
                return a;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error plc:{0}", ex.Message);
                return new object[] { 0, 0 };
            }
        }
    }
}
