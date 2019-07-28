using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RSoft.SocketSide
{
    public class UdpServer : IDisposable
    {
        public static UdpServer _uh;
        public Socket _Socket;
        public long BufferSize { private get; set; }

        #region events
        public delegate void ReceiveHandler(string message, EndPoint endPoint);
        public event ReceiveHandler Receive;
        public delegate void SentStateHandler(string message, int len);
        public event SentStateHandler SentState;
        public delegate void DisconnectHandler(EndPoint endPoint);
        public event DisconnectHandler Disconnect;
        public delegate void ErrorHandler(string methodName, Exception ex);
        public event ErrorHandler Error;
        #endregion

        private UdpServer(long bufferSize = 1024)
        {
            try
            {
                if (bufferSize <= 0)
                    throw new InvalidCastException("bufferSize is invalid !");

                BufferSize = bufferSize;

                _Socket =
    new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static UdpServer Create(long bufferSize = 1024) => _uh ?? (_uh = new UdpServer(bufferSize));

        public void Open(string ipAddress, int portNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(ipAddress))
                    throw new ArgumentNullException("ipAddress is null !");

                if (portNumber < 0)
                    throw new InvalidCastException("portNumber is invalid !");

                Open(new IPEndPoint(IPAddress.Parse(ipAddress), portNumber));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Open(IPEndPoint ipEndPoint)
        {
            try
            {
                if (ipEndPoint == null)
                    throw new ArgumentNullException("ipEndPoint is null !");

                _Socket.Bind(ipEndPoint);

                Packet packet = new Packet(BufferSize);

                _Socket.
                    BeginReceiveFrom(packet.Buffer, 0, packet.Buffer.Length,
                    SocketFlags.None, ref packet.EpFrom, new AsyncCallback(_Socket_Receive), packet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Send(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException("message is null !");

                var data = Encoding.ASCII.GetBytes(message);

                _Socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(_Socket_Send), message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendTo(string message, IPEndPoint endPoint)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException("message is null !");

                var data = Encoding.ASCII.GetBytes(message);

                _Socket.
                    BeginSendTo(data, 0, data.Length, SocketFlags.None, endPoint, new AsyncCallback(_Socket_SendTo), message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Close(int timeout = 1000)
        {
            try
            {
                _Socket.Close(timeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void _Socket_Receive(IAsyncResult result)
        {
            try
            {
                Packet packet = (Packet)result.AsyncState;

                int len = _Socket.EndReceiveFrom(result, ref packet.EpFrom);

                if (len <= 0)
                {
                    Disconnect(packet.EpFrom);
                }
                else
                {
                    var value = Encoding.ASCII.GetString(packet.Buffer, 0, len);

                    Receive(value, packet.EpFrom);
                }
            }
            catch (Exception ex)
            {
                Error("Receive", ex);
            }
            finally
            {
                Packet packet = new Packet(BufferSize);

                _Socket?.
                    BeginReceiveFrom(packet.Buffer, 0, packet.Buffer.Length,
                    SocketFlags.None, ref packet.EpFrom, new AsyncCallback(_Socket_Receive), packet);

                GC.Collect();
            }
        }

        private void _Socket_Send(IAsyncResult result)
        {
            try
            {
                var m = (string)result.AsyncState;
                var len = _Socket.EndSend(result);
                SentState(m, len);
            }
            catch (Exception ex) { Error("Send", ex); }
        }

        private void _Socket_SendTo(IAsyncResult result)
        {
            try
            {
                var m = (string)result.AsyncState;
                var len = _Socket.EndSendTo(result);
                SentState(m, len);
            }
            catch (Exception ex) { Error("SendTo", ex); }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
