using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace PcRemoteShutdown
{
  public partial class PcRemoteShutdown : ServiceBase
  {
    private const string IPAddressToListen = "192.168.129.1";

    private const int Port = 19191;

    private const uint MagicBeginning = 0x7b77ab88;

    private const uint SleepCommand = 0x112211ab;

    private const uint HibernateCommand = 0x348712ab;

    private Thread workingThread;

    public PcRemoteShutdown()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      workingThread = new Thread(DoWork) { IsBackground = true };
      workingThread.Start();
    }

    private void DoWork()
    {
      while (true)
      {

        var udpClient = new UdpClient();

        try
        {
          IPEndPoint broadcastAddress = new IPEndPoint(IPAddress.Parse(IPAddressToListen), Port);

          //udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
          //udpClient.ExclusiveAddressUse = true; // only if you want to send/receive on same machine.

          udpClient.Client.Bind(broadcastAddress);

          while (true)
          {
            try
            {
              IPEndPoint ep = new IPEndPoint(0, 0);
              var buffer = udpClient.Receive(ref ep);

              if (buffer.Length < 8)
              {
                continue;
              }

              var magicNumber = BitConverter.ToUInt32(buffer, 0);
              if (magicNumber != MagicBeginning)
              {
                //Skip trash
                continue;
              }
              var commandCode = BitConverter.ToUInt32(buffer, 4);

              HandleCommand(commandCode);

            }
            catch (ThreadAbortException tae)
            {
              break;
            }
            catch
            {

            }
          }
        }
        catch
        {

        }
        finally
        {
          udpClient.Close();
        }
      }
    }

    private void HandleCommand(uint commandCode)
    {
      try
      {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var exePath = Path.Combine(dir, "psshutdown.exe");
        
        if (commandCode == SleepCommand)
        {
          Process.Start(exePath, "-d -t 0 -accepteula");
        }
        else if (commandCode == HibernateCommand)
        {
          Process.Start(exePath, "-h -t 0 -accepteula");
        }
      }
      catch (Exception ex)
      {
        
      }
      
    }

    protected override void OnStop()
    {
      if (workingThread != null)
      {
        workingThread.Abort();
        workingThread = null;
      }
    }
  }
}
