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
using static SocketCANSharp.LibcNativeMethods;

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
				throw new Exception("Reading from SocketCan failed");
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
				throw new Exception("Writing to SocketCan failed");
		}

		public void close()
		{
			threadrun = false;
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
				throw new Exception("Failed to create socket.");
			}

			var ifr = new Ifreq(bus);
			int ioctlResult = LibcNativeMethods.Ioctl(socket.SafeHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
			if (ioctlResult == -1)
			{
				throw new Exception($"Failed to look up {bus} interface by name.");
			}

			var addr = new SockAddrCan(ifr.IfIndex);
			socket.Bind(addr);
			if(!socket.IsBound)
			{
				throw new Exception("Failed to bind to address.");
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
			LibcNativeMethods.pollfd pollfd = new LibcNativeMethods.pollfd();
			pollfd[] pollfdarray = [pollfd];
			pollfd.fd = (int)socket.SafeHandle.DangerousGetHandle();
			pollfd.events = (short)(LibcNativeMethods.POLLIN | LibcNativeMethods.POLLHUP | LibcNativeMethods.POLLERR);

			while (threadrun)
			{
				var res = LibcNativeMethods.poll( pollfdarray, (ulong)pollfdarray.Length, 500);
				if (res == -1)
				{
					throw new Exception("Error polling socket in CanOpen SocketCanDriver.");
				}

				if ((ushort)(pollfdarray[0].revents & LibcNativeMethods.POLLIN) == LibcNativeMethods.POLLIN)
				{
					Message rxmsg = canreceive();

					if (rxmsg.len != 0)
					{
						if (rxmessage != null)
							rxmessage(rxmsg);
					}
				}
			}
			socket.Close();
			socket.Dispose();
			socket = null;
		}
	}
}
