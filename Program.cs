using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace SocketERTramas
{
    class Program
    {

        static void Main(string[] args)
        {

            string trama = "CN00\u001c000000000118\u001c000000000018\u001c000000000000\u001c000002";
            // string trama = "CN01";
            // string trama = "CS00";
            // string trama = "CN00000000000118000000000018000000000000000002";

            // Comunicacion01 con = new Comunicacion01("10.0.0.21", 2018, "10.0.0.133", 7060);
            // Comunicacion01 con = new Comunicacion01("192.168.1.100", 2018, "192.168.1.110", 7060);

            Console.WriteLine("Introduce la ip Local ejemplo: 192.168.1.100");
            string ipLocal = Console.ReadLine();

            Console.WriteLine("Introduce la ip Remota ejemplo: 192.168.1.101");
            string ipRemota = Console.ReadLine();

            Comunicacion01 con = new Comunicacion01(ipLocal, 2018, ipRemota, 7060);
            Console.WriteLine("{0}->{1}", con.ConnectLocal, con.ConnectRemota);
            con.EnviarRecibirPOS(trama);

            Console.ReadKey();
        }
    }
}