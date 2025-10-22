using System;
using System.Runtime.InteropServices;
using System.Threading;

using SocketCANSharp;
using SocketCANSharp.Network;

using static libCanOpenSimple.IDriverInstance;

namespace libCanOpenSimple
{
    class DriverInstanceSocketCan : IDriverInstance
	{

		private	RawCanSocket socket;
		private volatile bool threadrun = true;
		private Thread rxthread;
		public event RxMessage rxmessage;

		public Message canreceive()
		{
			var msg = new Message();
			var canFrame = new CanFrame();
			int bytesRead = LibcNativeMethods.Read(socket.SafeHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
			if(bytesRead > 0)
			{ 
				msg.cob_id = (ushort)canFrame.CanId;
				msg.data = BitConverter.ToUInt64(canFrame.Data);
				msg.len = canFrame.Length;
			}
			else
			{
				msg.len = 0;
			}
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
			// Set the socket timeout to 500ms to keep the thread sensitive to the threadrun variable
			var timeout = new Timeval(0, 500000);
			if (-1 == LibcNativeMethods.SetSockOpt(socket.SafeHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeout, Marshal.SizeOf(timeout)))
			{
				throw new Exception("Failed to set socket timeout in DriverInstanceSocketCan");
			}

			while (threadrun)
			{
				Message rxmsg = canreceive();
				//Console.WriteLine($"{DateTime.Now.Second}.{DateTime.Now.Millisecond} socketCAN Driver rxmsg {rxmsg.len}");

				if (rxmsg.len != 0)
				{
					if (rxmessage != null)
						rxmessage(rxmsg);
				}
			}
			socket.Close();
			socket.Dispose();
			socket = null;
		}
	}
}
