using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SE_MTG
{
    internal class WebSocketServer
    {
        private HttpListener httpListener;

        public WebSocketServer(string hostname, int port)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{hostname}:{port}/");
        }

        public void Start()
        {
            httpListener.Start();
            Task.Run(ListenForClients);
        }

        public void Stop()
        {
            httpListener.Stop();
        }

        private async Task ListenForClients()
        {
            while (true)
            {
                var context = await httpListener.GetContextAsync();
                HandleClient(context);
            }
        }

        private async void HandleClient(HttpListenerContext context)
        {
            try
            {
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    await ProcessWebSocketRequest(webSocketContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling WebSocket request: " + ex.Message);
            }
        }

        private async Task ProcessWebSocketRequest(WebSocket webSocket)
        {
            try
            {
                // Continuously read incoming messages from the WebSocket
                while (webSocket.State == WebSocketState.Open)
                {
                    var completeMessage = new MemoryStream();
                    WebSocketReceiveResult receiveResult;
                    do
                    {
                        var buffer = new ArraySegment<byte>(new byte[1024]);
                        // Receive a segment of the message
                        receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        // Append this segment to the completeMessage stream
                        completeMessage.Write(buffer.Array, buffer.Offset, receiveResult.Count);
                    } while (!receiveResult.EndOfMessage); // Check if the message is complete

                    completeMessage.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

                    // Convert the complete message to a string
                    using (var reader = new StreamReader(completeMessage, Encoding.UTF8))
                    {
                        var messageString = reader.ReadToEnd();
                        Console.WriteLine($"Received message: {messageString}"); // Log the raw message content

                        // Now you can handle the full message stored in messageString
                        if (receiveResult.MessageType == WebSocketMessageType.Text)
                        {
                            if (messageString == "TreacheryUnveil1")
                            {
                                // Check if the call is required to be marshaled back to the UI thread
                                if (Form1.Instance.InvokeRequired)
                                {
                                    Form1.Instance.Invoke(new Action(() =>
                                    {
                                        Form1.Instance.Treachery1Unveil_Click(null, EventArgs.Empty);
                                    }));
                                }
                                else
                                {
                                    Form1.Instance.Treachery1Unveil_Click(null, EventArgs.Empty);
                                }

                                await SendMessageToWebSocket(webSocket, "ToggleTreacheryUnveil1");
                                Console.WriteLine("Handled TreacheryUnveil1 command.");
                            }
                            else if (messageString == "GetNumberOfPlayers")
                            {
                                int numberOfPlayers = 0;
                                // Assuming Form1.Instance and its properties are correctly implemented
                                if (Form1.Instance != null)
                                {
                                    numberOfPlayers = Form1.Instance.selectedPlayers; // Use correct property name
                                    Console.WriteLine("Number of players: " + numberOfPlayers);
                                }
                                await SendMessageToWebSocket(webSocket, numberOfPlayers.ToString());
                            }
                            else if (messageString.StartsWith("Player"))
                            {
                                // Assuming a method to handle player-specific requests
                                await HandlePlayerRequest(messageString, webSocket);
                            }
                            // Add additional conditions to handle other commands as needed
                        }
                    }

                    // Clear the MemoryStream after processing the message
                    completeMessage.SetLength(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing WebSocket request: " + ex.Message);
            }
            finally
            {
                // Close the WebSocket connection if it's still open
                if (webSocket.State == WebSocketState.Open)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }





        private async Task SendMessageToWebSocket(WebSocket webSocket, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // Placeholder for the HandlePlayerRequest method, assuming you implement it based on your application logic
        private async Task HandlePlayerRequest(string command, WebSocket webSocket)
        {
            // Your logic to handle player-specific requests, e.g., parsing the command to extract player number
            // and sending back the corresponding player information
        }
    }
}
