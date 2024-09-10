using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketCANSharp;
using SocketCANSharp.Network;

using static libCanOpenSimple.IDriverInstance;

namespace libCanOpenSimple
{
    class DriverInstanceSocketCan : IDriverInstance
	{

		private	RawCanSocket socket;
		private bool threadrun = true;
		private Thread rxthread;
		public event RxMessage rxmessage;

		public Message canreceive()
		{
			var msg = new Message();
			var frm = new CanFrame();
			int bytesRead = socket.Read(out frm);
			msg.cob_id = (ushort)frm.CanId;
			msg.data = BitConverter.ToUInt64(frm.Data);
			msg.len = frm.Length;
			if (bytesRead == -1)
				Console.WriteLine("Reading fromCAN Socket failed");
			return msg;
		}

		public void cansend(Message msg)
		{
			var frm = new CanFrame();
			frm.CanId = msg.cob_id;
			frm.Data = BitConverter.GetBytes(msg.data);
			frm.Length = msg.len;
			int bytesWritten = socket.Write(frm);
			if (bytesWritten == -1)
				Console.WriteLine("Writing to CAN Socket failed");
		}

		public void close()
		{
			socket.Close();
			socket.Dispose();
			socket = null;
		}

		public void enumerate()
		{
			throw new NotImplementedException();
		}

		public bool isOpen()
		{
			return socket.IsBound;
		}

		public bool open(string bus, BUSSPEED speed)
		{
			socket = new RawCanSocket();
			if (socket.SafeHandle.IsInvalid)
			{
				Console.WriteLine("Failed to create socket.");
				return false;
			}

			var ifr = new Ifreq(bus);
			int ioctlResult = LibcNativeMethods.Ioctl(socket.SafeHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
			if (ioctlResult == -1)
			{
				Console.WriteLine($"Failed to look up {bus} interface by name.");
				return false;
			}

			var addr = new SockAddrCan(ifr.IfIndex);
			socket.Bind(addr);
			if(!socket.IsBound)
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
				}
			}
			catch (Exception ex) 
			{
				Console.WriteLine($"Exception thrown in rx thread worker {ex}");
			}
		}

	}
}
