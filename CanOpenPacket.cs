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

using System;

namespace libCanOpenSimple
{
	/// <summary>
	/// C# representation of a CanPacket, containing the COB the length and the data. RTR is not supported
	/// as it's pretty much not used on CanOpen, but this could be added later if necessary
	/// </summary>
	public class CanOpenPacket
    {
        public UInt16 cob;
        public byte len;
        public byte[] data;
        public bool bridge = false;

        public CanOpenPacket()
        {
        }

        /// <summary>
        /// Construct C# Canpacket from a CanFestival message
        /// </summary>
        /// <param name="msg">A CanFestival message struct</param>
        public CanOpenPacket(Message msg,bool bridge=false)
        {
            cob = msg.cob_id;
            len = msg.len;
            data = new byte[len];
            this.bridge = bridge;

            byte[] temp = BitConverter.GetBytes(msg.data);
            Array.Copy(temp, data, msg.len);
        }

        /// <summary>
        /// Convert to a CanFestival message
        /// </summary>
        /// <returns>CanFestival message</returns>
        public Message ToMsg()
        {
            Message msg = new Message();
            msg.cob_id = cob;
            msg.len = len;
            msg.rtr = 0;

            byte[] temp = new byte[8];
            Array.Copy(data, temp, len);
            msg.data = BitConverter.ToUInt64(temp, 0);

            return msg;

        }

        /// <summary>
        /// Dump current packet to string
        /// </summary>
        /// <returns>Formatted string of current packet</returns>
        public override string ToString()
        {
            string output = string.Format("{0:x3} {1:x1}", cob, len);

            for (int x = 0; x < len; x++)
            {
                output += string.Format(" {0:x2}", data[x]);
            }
            return output;
        }
    }
}
