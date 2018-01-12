# IdealWebSocket
1.What is IdealWebsocket
  IdealWebSocket is a library that help you to build your own WebSocketServer easily.it hide the socket layer details so you can focus on your application. 
2.How to use IdealWebSocket
  it is really simple to use this library.The server is an abstract object that provided the message service.you need just to implemnet the abstract class WebSocketServer.cs
  and to do the application  logic as showing in the follow.
        
        public class MyWebSocketServer : IdealWebSocket.ServerWebSocket.WebSocketServer
        {
            public ChatRoom Room = new ChatRoom();

            public MyWebSocketServer()
            {

            }

            protected override void OnSessionClose(WebSocketSession session, WebSocketReceiveError error)
            {
                
            }

            protected override void OnSessionConnected(WebSocketSession session)
            {
                
            }

            protected override void OnSessionNewMessage(WebSocketSession session, WebSocketMessage message)
            {
                        
            }
        }
then on your program,initial your custom websocketserver object,set the port to listen then open it.

        MyWebSocketServer serverSocket = new MyWebSocketServer();
        serverSocket.SetPort(60328);
        serverSocket.Open();
        Console.WriteLine("WebSocketServer Opened");
        Console.ReadKey();
        
implement your logic on OnSessionClose,OnSessionConnected,OnSessionNewMessage method.more details can see in the sample WebSocketServerApp
