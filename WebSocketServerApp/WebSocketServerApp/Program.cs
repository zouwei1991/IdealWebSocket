using System;
using System.Text;

namespace WebSocketServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //initial an instance of your custome websocket server
            MyWebSocketServer serverSocket = new MyWebSocketServer();
            //set the port to listen
            serverSocket.SetPort(60328);
            //open the server
            serverSocket.Open();
            Console.WriteLine("WebSocketServer Opened");
            Console.ReadKey();
        }
    }
}
