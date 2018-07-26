/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
#if !WINDOWS_UWP
using WebSocketSharp; 
#endif

namespace RosSharp.RosBridgeClient.Protocols
{
    public class WebSocketSharpProtocol: IProtocol
    {
        public event EventHandler OnReceive;

#if !WINDOWS_UWP
        private WebSocket WebSocket; 
#endif

        public WebSocketSharpProtocol(string url)
        {
#if !WINDOWS_UWP
            WebSocket = new WebSocket(url);
            WebSocket.OnMessage += Receive; 
#endif
        }
                
        public void Connect()
        {
#if !WINDOWS_UWP
            WebSocket.Connect();

#endif
        }

        public void Close()
        {
#if !WINDOWS_UWP
                WebSocket.Close();
#endif
        }

        public bool IsAlive()
        {
#if !WINDOWS_UWP
            return WebSocket.IsAlive;
#else
            return false;
#endif
        }

        public void Send(byte[] data)
        {
#if !WINDOWS_UWP
            WebSocket.SendAsync(data, null); 
#endif
        }

#if !WINDOWS_UWP
        private void Receive(object sender, WebSocketSharp.MessageEventArgs e)
        {
            OnReceive.Invoke(sender, new MessageEventArgs(e.RawData));
        } 
#endif
    }
}
