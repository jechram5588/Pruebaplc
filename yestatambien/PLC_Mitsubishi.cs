using HslCommunication;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace yestatambien
{
    public class PLC_Mitsubishi
    {
        private MelsecA1ENet melsec_net = null;
        public Log Logger;
        public enum TipoDato
        {
            Float, Entero, String, Boolean
        }

        public PLC_Mitsubishi(string ServiceName, MelsecA1ENet plc)
        {
            melsec_net = plc;
            Logger = new Log(ServiceName);
        }

        #region PLC_Methods
        public bool ConectaPLC(string PLC_IP, string Puerto)
        {
            melsec_net.ConnectTimeOut = 500;
            melsec_net.ReceiveTimeOut = 500;

            if (!System.Net.IPAddress.TryParse(PLC_IP, out System.Net.IPAddress address))
            {
                Logger.PrimaryLog("Conección a PLC", "IP no valida", EventLogEntryType.Error, true);
                return false;
            }

            melsec_net.IpAddress = PLC_IP;

            if (!int.TryParse(Puerto, out int port))
            {
                Logger.PrimaryLog("Conección a PLC", "Puerto Erroneo", EventLogEntryType.Error, true);
                return false;
            }

            melsec_net.Port = Convert.ToInt32(Puerto);
            melsec_net.ConnectClose();

            try
            {
                OperateResult connect = melsec_net.ConnectServer();
                if (connect.IsSuccess)
                {
                    return true;
                }
                else
                {
                    Logger.PrimaryLog("Conección a PLC", "No se logro conectar", EventLogEntryType.Error, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.PrimaryLog("Conección a PLC", ex.Message.ToString(), EventLogEntryType.Error, true);
                return false;
            }
        }
        public void DesconectaPLC()
        {
            melsec_net.ConnectClose();
        }
        public bool EscribePLC(TipoDato Tipo, string Variable, string Valor)
        {
            OperateResult result = new OperateResult();
            try
            {
                switch (Tipo)
                {
                    case TipoDato.Float:
                        result = melsec_net.Write(Variable, float.Parse(Valor));
                        break;
                    case TipoDato.Entero:
                        result = melsec_net.Write(Variable, int.Parse(Valor));
                        break;
                    case TipoDato.String:
                        result = melsec_net.Write(Variable, Valor);
                        break;
                    case TipoDato.Boolean:
                        result = melsec_net.Write(Variable, new bool[] { bool.Parse(Valor) });
                        break;
                    default:
                        break;
                }
                if (result.IsSuccess)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.PrimaryLog("Escribe PLC", string.Format("{0}", ex.Message), EventLogEntryType.Error, true);
                return false;
            }
        }
        public string LeePLC(TipoDato Tipo, string Variable, int cant)
        {
            string res = "";
            OperateResult result = new OperateResult();
            try
            {
                switch (Tipo)
                {
                    case TipoDato.Float:
                        res = ReadResultRender(melsec_net.ReadFloat(Variable));
                        break;
                    case TipoDato.Entero:
                        res = ReadResultRender(melsec_net.ReadInt16(Variable));
                        break;
                    case TipoDato.String:
                        res = ReadResultRender(melsec_net.ReadString(Variable, Convert.ToUInt16(cant)));
                        break;
                    case TipoDato.Boolean:
                        res = ReadResultRender(melsec_net.ReadBool(Variable));
                        break;
                    default:
                        break;
                }
                if (result.IsSuccess)
                {
                    Logger.PrimaryLog("Leer PLC", string.Format("{0} {1} {2}", Tipo, Variable, res), EventLogEntryType.Information, false);
                    return res;
                }
                else
                    return res;
            }
            catch (Exception ex)
            {
                Logger.PrimaryLog("Leer PLC", string.Format("{0}", ex.Message), EventLogEntryType.Error, true);
                return "";
            }
        }
        public static string ReadResultRender<T>(OperateResult<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Content.ToString();
            }
            else
            {
                return "";
            }
        }

        public string HexStrToAscci(byte[] buffer)
        {
            Encoding enc8 = Encoding.UTF8;
            return enc8.GetString(buffer);
        }
        public string ReverseString2a1(string s)
        {
            /* acomodamos el string leido del plc*/
            char[] array = new char[s.Length];
            string last = "";

            if (!(s.Length % 2 == 0))
                last = s.Substring(s.Length - 1, 1);

            for (int i = 0; i < s.Length - 1; i++)
            {
                if (i % 2 == 0)
                    array[i + 1] = s[i];
                else
                    array[i - 1] = s[i];
            }
            if (last != "")
                array[s.Length - 1] = last.ToArray()[0];
            /*separamos Rack type y rack number*/
            //J59W RH 16 #101     50         N

            return new string(array);
        }

        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
        public byte[] FromHex(string hex)
        {
            hex = hex.Replace(" ", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
        #endregion
    }
    public class Log
    {
        string App = "";
        bool bDebug = false;
        string LogFolder = "";
        public Log(string app)
        {
            App = app;
            var appSettings = ConfigurationManager.AppSettings;
            bDebug = Convert.ToBoolean(appSettings["DebugMode"]);
            //try
            //{
            //    if (!EventLog.SourceExists(App))
            //    {
            //        EventLog.CreateEventSource(App, App);
            //    }
            //}
            //catch { }
            LogFolder = AppDomain.CurrentDomain.BaseDirectory + "Logs";
            if (!System.IO.Directory.Exists(LogFolder))
                System.IO.Directory.CreateDirectory(LogFolder);
        }

        public void PrimaryLog(string origen, string evento, EventLogEntryType tipo, bool Forzarlog)
        {
            if (bDebug)
            {
                Debug.Print(string.Format("{0}:\t{1}->{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), origen, evento));
                Console.WriteLine(string.Format("{0}:\t{1}->{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), origen, evento));
            }
        }
    }

}
