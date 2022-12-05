using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Profinet.Melsec;

namespace yestatambien
{
    class Program
    {
        static void Main(string[] args)
        {
            var hilo = new Thread(() =>
            {
                ClassPrin conn = new ClassPrin();
                conn.inicio();
            });
        }
    }

    public class ClassPrin
    {

        public ClassPrin()
        {
        }

        public void inicio() {
            for (int i = 0; i < 1; i++)
            {
                MelsecA1ENet melsec_net = new MelsecA1ENet();
                var task = Task.Factory.StartNew(() => {
                    lectura(i,melsec_net);
                });
            }
        }

        public void lectura(int i,MelsecA1ENet plc)
        {
            PLC_Mitsubishi mit = new PLC_Mitsubishi(i.ToString(), plc);
            while (true) {
                if (mit.ConectaPLC("192.168.1.100","5000")) {
                    string lectura = mit.LeePLC(PLC_Mitsubishi.TipoDato.Float, "D200", 0);
                    Console.WriteLine($"PLC {mit.GetHashCode()}:\t{lectura}");
                }
            }
        }
    }
}
