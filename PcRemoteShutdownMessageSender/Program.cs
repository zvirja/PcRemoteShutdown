using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PsRemoteShutdownMessageSender
{

  class Program
  {
    private const int Port = 19191;
    private const uint MagicBeginning = 0xab77ab88;


    static void Main(string[] args)
    {
      var udpClinet = new UdpClient();

      var msg = new byte[8];
      var magic = BitConverter.GetBytes(MagicBeginning);

      Array.Copy(magic,0,msg,0,4);


      udpClinet.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("192.168.129.255"), Port));



    }
  }
}
