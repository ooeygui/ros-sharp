#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Foundation;
using Windows.Web;
using Windows.Storage.Streams;

using System.Collections;
using System.Collections.Generic;
using System;


namespace RosSharp.RosBridgeClient.Protocols
{
    public class WebSocketUWPProtocol : IProtocol
    {
        MessageWebSocket WebSocket;
        Uri Url;
        DataWriter MessageWriter;

        public event EventHandler OnReceive;
        int p = (int)Environment.OSVersion.Platform;
        public WebSocketUWPProtocol(string Url)
        {
            this.Url = TryGetUri(Url);
            WebSocket = new MessageWebSocket();
            WebSocket.MessageReceived += Receive;
            WebSocket.Closed += Close;


        }

        public bool isAlive = false;

        public bool IsAlive()
        {
            return isAlive;
        }

        public void Connect()
        {
            WebSocket.ConnectAsync(this.Url).Completed = (source, status) =>
            {
                if (status == AsyncStatus.Completed)
                {
                    MessageWriter = new DataWriter(WebSocket.OutputStream);
                    isAlive = true;
                }
            };
        }

        public void Close()
        {
            if (WebSocket != null)
            {
                WebSocket.Dispose();
                WebSocket = null;
                isAlive = false;
            }
        }

        public async void Send(byte[] data)
        {
            if (WebSocket != null && MessageWriter != null)
            {
                try
                {
                    MessageWriter.WriteBytes(data);
                    await MessageWriter.StoreAsync();
                }
                catch
                {
                    return;
                }
            }
        }

        void Receive(MessageWebSocket FromSocket, MessageWebSocketMessageReceivedEventArgs InputMessage)
        {
            try
            {
                using (var reader = InputMessage.GetDataReader())
                {
                    var messageLength = InputMessage.GetDataReader().UnconsumedBufferLength;
                    byte[] receivedMessage = new byte[messageLength];
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    reader.ReadBytes(receivedMessage);
                    OnReceive.Invoke(this, new MessageEventArgs(receivedMessage));

                }
            }
            catch
            {
                return;
            }
        }

        static System.Uri TryGetUri(string uriString)
        {
            Uri webSocketUri;
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out webSocketUri))
                throw new System.Exception("Error: Invalid URI");

            // Fragments are not allowed in WebSocket URIs.
            if (!String.IsNullOrEmpty(webSocketUri.Fragment))
                throw new System.Exception("Error: URI fragments not supported in WebSocket URIs.");

            // Uri.SchemeName returns the canonicalized scheme name so we can use case-sensitive, ordinal string
            // comparison.
            if ((webSocketUri.Scheme != "ws") && (webSocketUri.Scheme != "wss"))
                throw new System.Exception("Error: WebSockets only support ws:// and wss:// schemes.");

            return webSocketUri;
        }
    }
};

#endif