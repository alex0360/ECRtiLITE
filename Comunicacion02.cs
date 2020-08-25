using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketERTramas
{
    public class Comunicacion02
    {
        System.Net.IPEndPoint connect;
        string ex = null;
        Socket listener;
        Socket socket;
        byte[] SentAndReceivedBytes = new byte[1024];
        System.Collections.Generic.List<string> closures = new List<string>();


        public void Conectar(string ipLocal = "", int puertoLocal = 2018, string ipRemota = "127.0.0.1", int puertoRemoto = 7060)
        {

            Socket remoteEndPoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {

                connect = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipRemota), puertoRemoto);

                if (SendENQ())

                    if (SendSYN(out SentAndReceivedBytes))
                    {

                    }
            }
            catch (Exception e) { ex = e.Message.ToString(); }
            finally { remoteEndPoint.Close(); }

        }

        private void enviar(Socket remoteEndPoint)
        {
            int retransmissionTries = 0;
            string recibedData = null;
            char controlCharacter = '\0';
            string frame = "\u0002CN00\u001c000000000118\u001c000000000018\u001c000000000000\u001c000003\u0003";

            listener.Bind(connect);
            listener.Listen(5);
            listener.ReceiveTimeout = 30789;
            listener.SendTimeout = 30789;
            socket = listener.Accept();

            remoteEndPoint.ReceiveTimeout = 30789;
            remoteEndPoint.SendTimeout = 3078;

            while (retransmissionTries < 2)
            {
                retransmissionTries++;

                try
                {
                    socket.LingerState = new LingerOption(true, 10);
                    socket.Receive(SentAndReceivedBytes);
                    recibedData += System.Text.Encoding.ASCII.GetString(SentAndReceivedBytes);

                    if (recibedData == "")
                    {
                        //LogNewTransaction(posToEcr, LogDto.Requests.Received, GetRequestType(ControlCharacters.ENQ), recibedData);

                        SentAndReceivedBytes = (new System.Text.ASCIIEncoding()).GetBytes(frame);
                        socket.Send(SentAndReceivedBytes);
                        var port = ((System.Net.IPEndPoint)socket.RemoteEndPoint).Port;
                        //LogNewTransaction(ecrToPos, LogDto.Requests.Sent, Messages.Information.Frame(), clearFrame);

                        socket.Receive(SentAndReceivedBytes);
                        controlCharacter = Convert.ToChar(SentAndReceivedBytes[0]);

                        // If transaction is valid
                        if (controlCharacter == '')
                        {
                            //LogNewTransaction(posToEcr, LogDto.Requests.Received, GetRequestType(ControlCharacters.ACK), ControlCharacters.ACK.ToString());

                            do
                            {
                                if (socket.Connected)
                                {
                                    SentAndReceivedBytes = new byte[1024];
                                    socket.Receive(SentAndReceivedBytes);

                                    controlCharacter = Convert.ToChar(SentAndReceivedBytes[0]);

                                    if (!controlCharacter.Equals(''))//EOT
                                    {
                                        recibedData = System.Text.Encoding.ASCII.GetString(SentAndReceivedBytes).Replace("\0", "");

                                        if (frame.Equals("CS00"))
                                        {
                                            var cardCleanedResponse = CleanedDataResponse(recibedData);

                                            // LogNewTransaction(posToEcr, LogDto.Requests.Received, Messages.Information.Frame(), recibedData);

                                            SentAndReceivedBytes = FrameToBytes("");
                                            socket.Send(SentAndReceivedBytes);

                                            // LogNewTransaction(posToEcr, LogDto.Requests.Received, Messages.Information.Frame(), recibedData);

                                        }

                                        if (frame.Contains(ClosingBatch.Substring(0, 5)))
                                            closures.Add(recibedData);

                                        //LogNewTransaction(posToEcr, LogDto.Requests.Received, Messages.Information.Frame(), recibedData);
                                    }
                                    else
                                    {
                                        //LogNewTransaction(posToEcr, LogDto.Requests.Received, GetRequestType(controlCharacter), controlCharacter.ToString());
                                    }

                                    SentAndReceivedBytes = FrameToBytes("");
                                    socket.Send(SentAndReceivedBytes);
                                    //LogNewTransaction(ecrToPos, LogDto.Requests.Sent, GetRequestType(ControlCharacters.ACK), ControlCharacters.ACK.ToString());
                                }
                            } while (!controlCharacter.Equals(''));
                        }
                    }
                }
                catch (TimeoutException)
                {

                }

                listener.Close();
            }
        }

        private bool SendENQ(string ENQ = "")
        {//  string ENQ = "\u0005";
            using (var remoteEndPoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                remoteEndPoint.ReceiveTimeout = 30789;
                remoteEndPoint.SendTimeout = 30789;
                remoteEndPoint.Connect(connect);

                byte[] buffer = System.Text.Encoding.Default.GetBytes(ENQ);

                remoteEndPoint.Send(buffer);
                remoteEndPoint.Receive(buffer);

                var recibedData = System.Text.Encoding.ASCII.GetString(buffer); // Se espera: ACK =  = \u0006
                Console.WriteLine("RecibedData: " + recibedData.ToString());

                if (recibedData.Equals(""))
                    return true;
                else
                    return false;
            }
        }

        private bool SendSYN(out byte[] buffer, string SYN = "")
        {
            using (var remoteEndPoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                remoteEndPoint.ReceiveTimeout = 30789;
                remoteEndPoint.SendTimeout = 30789;
                remoteEndPoint.Connect(connect);

                buffer = System.Text.Encoding.Default.GetBytes(SYN);

                remoteEndPoint.Send(buffer);
                remoteEndPoint.Receive(buffer);

                var recibedData = System.Text.Encoding.ASCII.GetString(buffer); // Se espera: ACK =  = \u0006

                if (recibedData.Equals(""))
                    return true;
                else
                    return false;
            }
        }


        // Funciones<<<->>>
        static byte[] FrameToBytes(string frame)
        {
            // Convert String to Bytes
            var bytes = (new System.Text.ASCIIEncoding()).GetBytes(frame);

            // if the length of the frame is equal to one then return the byte array.
            if (frame.Length == 1) return bytes;

            // Add a byte at the end to insert the LRC
            var buffer = new byte[bytes.Length + 1];
            bytes.CopyTo(buffer, 0);

            // Calculate the LCR and add to the end of the array.
            buffer[buffer.Length - 1] = GetLRC(bytes);

            // Return the Converted Array
            return buffer;
        }
        static string CleanedDataResponse(string data)
        {
            return data.Replace("", "")
                .Replace("", "")
                .Replace("\0", "");
        }
        private static byte GetLRC(System.Collections.Generic.IReadOnlyList<byte> data)
        {
            var checksum = data[1];
            for (var i = 2; i <= data.Count - 1; i++) checksum ^= data[i];
            return checksum;
        }
        public static string ClosingBatch => "CN01{0}";
    }
}
