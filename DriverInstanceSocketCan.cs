using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using libCanOpenSimple.SocketCan;

using static libCanOpenSimple.IDriverInstance;

namespace libCanOpenSimple
{
    class DriverInstanceSocketCan : IDriverInstance
	{

		private static SafeFileDescriptorHandle socketHandle;
		private bool threadrun = true;
		private Thread rxthread;
		public event RxMessage rxmessage;

		public Message canreceive()
		{
			var msg = new Message();
			int bytesRead = LibcNativeMethods.Read(socketHandle, ref msg, Marshal.SizeOf(typeof(Message)));
			if (bytesRead == -1)
				Console.WriteLine("Reading fromCAN Socket failed");
			return msg;
		}

		public void cansend(Message msg)
		{
			int bytesWritten = LibcNativeMethods.Write(socketHandle, ref msg, Marshal.SizeOf(typeof(Message)));
			if (bytesWritten == -1)
				Console.WriteLine("Writing to CAN Socket failed");
		}

		public void close()
		{
			socketHandle.Close();
			socketHandle.Dispose();
			socketHandle = null;
		}

		public void enumerate()
		{
			throw new NotImplementedException();
		}

		public bool isOpen()
		{
			return socketHandle != null;
		}

		public bool open(string bus, BUSSPEED speed)
		{
			socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
			if (socketHandle.IsInvalid)
			{
				Console.WriteLine("Failed to create socket.");
				return false;
			}

			var ifr = new Ifreq(bus);
			int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
			if (ioctlResult == -1)
			{
				Console.WriteLine("Failed to look up interface by name.");
				return false;
			}

			var addr = new SockAddrCan(ifr.IfIndex);
			int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
			if (bindResult == -1)
			{
				Console.WriteLine("Failed to bind to address.");
				return false;
			}


			rxthread = new System.Threading.Thread(rxthreadworker);
			rxthread.Start();

			return true;
		}

		/// <summary>
		/// Private worker thread to keep the rxmessage() function pumped
		/// </summary>
		private void rxthreadworker()
		{
			try
			{
				while (threadrun)
				{

					Message rxmsg = canreceive();

					if (rxmsg.len != 0)
					{
						if (rxmessage != null)
							rxmessage(rxmsg);
					}

					//System.Threading.Thread.Sleep(0);
				}
			}
			catch
			{

			}
		}

	}
}
