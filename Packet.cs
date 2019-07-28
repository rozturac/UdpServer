using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RSoft.SocketSide
{
    internal class Packet
    {
        internal byte[] Buffer;
        internal EndPoint EpFrom = new IPEndPoint(IPAddress.Any, 0);
        internal Packet(long bufferSize)
        {
            Buffer = new byte[bufferSize];
        }
    }
}
