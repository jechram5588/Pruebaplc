using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pruebameperoesta
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

    public class ClassPrin {

        ILog Logger = LogManager.GetLogger("tst");

        public ClassPrin() { 
        }

        public void inicio() {
            Corping.PLCConnector.PLC _plc = new Corping.PLCConnector.PLC("192.168.5.200", 5000, true, 15000);
            for (int i = 0; i < 1; i++)
            {
                var task = Task.Factory.StartNew(() => {
                    lectura(_plc);
                });
            }
           
        }

        public void lectura(Corping.PLCConnector.PLC _plc) {
            int tempLeido = 200;
            ConnectionPLC cplc = new ConnectionPLC(200, 10, 20);
            while (true)
            {
                object lec = cplc.connected(_plc, tempLeido);
                Console.WriteLine(lec);
                //Logger.InfoFormat("Data Leido {0}",lec);
                Thread.Sleep(5000);
            }


        }

    }
}
