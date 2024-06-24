using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static List<Socket> clients = new List<Socket>();
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint endpoint = new IPEndPoint(ip, 12345);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(endpoint);
            serverSocket.Listen(5);
            Console.WriteLine("Сервер запущен на {0}:{1}", ip, endpoint.Port);

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                clients.Add(clientSocket);
                Console.WriteLine("Подключен клиент: {0}", clientSocket.RemoteEndPoint);
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        static void HandleClient(Socket clientSocket)
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0) break;
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine("Получено сообщение: {0}", message);
                    BroadcastMessage(message, clientSocket);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Клиент отключился: {0}", clientSocket.RemoteEndPoint);
                    clients.Remove(clientSocket);
                    clientSocket.Close();
                    break;
                }
            }
        }

        static void BroadcastMessage(string message, Socket excludeSocket)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            foreach (Socket socket in clients)
            {
                if (socket != excludeSocket)
                {
                    socket.Send(buffer);
                }
            }
        }
    }
}