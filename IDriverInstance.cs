/*
    This file is part of libCanopenSimple.
    libCanopenSimple is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    libCanopenSimple is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with libCanopenSimple.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2017 Robin Cornelius <robin.cornelius@gmail.com>
*/


namespace libCanOpenSimple
{
	public interface IDriverInstance
	{
		/// <summary>
		/// CANOpen message recieved callback, this will be fired upon any recieved complete message on the bus
		/// </summary>
		/// <param name="msg">The CanOpen message</param>
		public delegate void RxMessage(Message msg, bool bridge = false);

		event RxMessage rxmessage;

		Message canreceive();
		void cansend(Message msg);
		void close();
		void enumerate();
		bool isOpen();

		/// <summary>
		/// Open a connection to a particular CAN bus
		/// </summary>
		/// <param name="bus">Something like can0 or vcan0 on linux</param>
		/// <param name="speed">Note that speed is preconfigured for SocketCan and can not be changed - this parameter is ignored</param>
		/// <returns></returns>
		bool open(string bus, BUSSPEED speed);
	}
}