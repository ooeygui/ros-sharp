#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Foundation;
using Windows.Web;
using Windows.Storage.Streams;
#endif

using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;


namespace RosSharp.RosBridgeClient.Protocols
{
    public class WebSocketUWPProtocol : IProtocol
    {

        public event EventHandler OnReceive;

#if WINDOWS_UWP
        MessageWebSocket WebSocket;
        Uri Url;
        DataWriter MessageWriter;
#endif


        public WebSocketUWPProtocol(string Url)
        {
#if WINDOWS_UWP
            this.Url = TryGetUri(Url);
            WebSocket = new MessageWebSocket();
            WebSocket.MessageReceived += Receive;
            Debug.Log("end of constructor");
#endif
        }

        public bool isAlive = false;

        public bool IsAlive()
        {
            return isAlive;
        }

        public void Connect()
        {
#if WINDOWS_UWP
            Debug.Log("begining of connect");

            WebSocket.ConnectAsync(this.Url).Completed = (source, status) =>
                {
                    if (status == AsyncStatus.Completed)
                    {
                        MessageWriter = new DataWriter(WebSocket.OutputStream);
                        isAlive = true;
                    }
                };
            Debug.Log("end of connect");
#endif
        }

        public void Close()
        {
#if WINDOWS_UWP
            if (WebSocket != null)
            {
                WebSocket.Dispose();
                WebSocket = null;
                isAlive = false;
            } 
#endif
        }

        public async void Send(byte[] data)
        {
#if WINDOWS_UWP
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
#endif
        }

#if WINDOWS_UWP
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
#endif

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
