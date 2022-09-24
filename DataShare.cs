using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace HTTPServer
{
    class Client
    {

        private void SendHeaders(TcpClient Client, int StatusCode)
        {
            string Headers = $"HTTP/1.1 {StatusCode}\nContent-Type: application/json\n\n";
            byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
            Client.GetStream().Write(HeadersBuffer, 0, HeadersBuffer.Length);
        }
        private void SendRequest(TcpClient Client, string Content, int StatusCode)
        {

            if (Client.Connected is false)
            {
                return;
            }

            SendHeaders(Client, StatusCode);

            byte[] Buffer = Encoding.ASCII.GetBytes(Content);
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Close();
        }

        private string ParseRequest(TcpClient Client)
        {
            string Request = "";
            byte[] Buffer = new byte[1024];
            int Count;

            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);

                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }

            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (ReqMatch == Match.Empty)
            {
                return "";
            }

            return ReqMatch.Groups[1].Value;
        }

        public Client(TcpClient Client)
        {

            string RequestUri = ParseRequest(Client);
            RequestUri = Uri.UnescapeDataString(RequestUri);

            switch (RequestUri)
            {
                case "/get_all_data":
                    SendRequest(Client, "{\"around_content\": [{\"portal_1\": {\"x\": 154, \"y\": 643}}]}", 200);
                    break;
            }
            Client.Close();
        }
    }

    class Server
    {
        TcpListener Listener;

        public Server(int Port)
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();

            while (true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
            }
        }

        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }

        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }

        static void StartServer(Object StateInfo)
        {
            new Server(4000);
        }

        static void Main(string[] args)
        {
            int MaxThreadsCount = Environment.ProcessorCount;

            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            ThreadPool.SetMinThreads(2, 2);

            ThreadPool.QueueUserWorkItem(new WaitCallback(StartServer));

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Working and not blocking Main thread");
            }

        }
    }
}