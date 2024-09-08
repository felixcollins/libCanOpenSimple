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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace libCanOpenSimple
{
	/// <summary>
	/// DriverInstace represents a specific instance of a loaded canfestival driver
	/// </summary>
	/// 

	public class DriverInstance
    {

        private bool threadrun = true;
        System.Threading.Thread rxthread;

        /// <summary>
        /// CANOpen message recieved callback, this will be fired upon any recieved complete message on the bus
        /// </summary>
        /// <param name="msg">The CanOpen message</param>
        public delegate void RxMessage(Message msg,bool bridge=false);
        public event RxMessage rxmessage;

        /// <summary>
        /// CanFestival message packet. Note we set data to be a UInt64 as inside canfestival its a fixed char[8] array
        /// we cannout use fixed arrays in C# without UNSAFE so instead we just use a UInt64
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
        public struct Message
        {
            public UInt16 cob_source_id; /**< message's ID */
            public UInt16 cob_id; /**< message's ID */
            public byte rtr;       /**< remote transmission request. (0 if not rtr message, 1 if rtr message) */
            public byte len;       /**< message's length (0 to 8) */
            public UInt64 data;
        }

        /// <summary>
        /// This contains the bus name on which the can board is connected and the bit rate of the board
        /// </summary>

        [StructLayout(LayoutKind.Sequential)]
        public struct struct_s_BOARD
        {

            [MarshalAs(UnmanagedType.LPStr)]
            public String busname;  /**< The bus name on which the CAN board is connected */

            [MarshalAs(UnmanagedType.LPStr)]
            public String baudrate; /**< The board baudrate */
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct struct_s_DEVICES
        {
            public UInt32 id;

            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct UnmanagedStruct
        {
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeConst = 100)]
            public IntPtr[] listOfStrings;

            public IEnumerable<string> Strings
            {
                get
                {
                    return listOfStrings.Select(x => Marshal.PtrToStringAnsi(x));
                }
            }
        }

        UnmanagedStruct enumerationresult;

        public delegate byte canReceive_T(IntPtr handle, IntPtr msg);
        private canReceive_T canReceive;

        public delegate byte canSend_T(IntPtr handle, IntPtr msg);
        private canSend_T canSend;

        public delegate IntPtr canOpen_T(IntPtr brd);
        private canOpen_T canOpen;

        public delegate UInt32 canClose_T(IntPtr handle);
        private canClose_T canClose;

        public delegate byte canChangeBaudRate_T(IntPtr handle, string rate);
        private canChangeBaudRate_T canChangeBaudrate;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void canEnumerateDelegate_T(
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 1)]
        string[] values,
        int valueCount);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void canEnumerate_T(canEnumerateDelegate_T callback);
        private canEnumerate_T canEnumerate;

        private IntPtr instancehandle = IntPtr.Zero;
        IntPtr brdptr;

        struct_s_BOARD brd;

        /// <summary>
        /// Create a new DriverInstance, this class provides a wrapper between the C# world and the C API dlls from canfestival that
        /// provide access to the CAN hardware devices. The exposed delegates represent the 5 defined entry points that all can festival
        /// drivers expose to form the common driver interface API. Usualy the DriverLoader class will directly call this constructor.
        /// </summary>
        /// <param name="canReceive">pInvoked delegate for canReceive function</param>
        /// <param name="canSend">pInvoked delegate for canSend function</param>
        /// <param name="canOpen">pInvoked delegate for canOpen function</param>
        /// <param name="canClose">pInvoked delegate for canClose function</param>
        /// <param name="canChangeBaudrate">pInvoked delegate for canChangeBaudrate functipn</param>
        public DriverInstance(canReceive_T canReceive, canSend_T canSend, canOpen_T canOpen, canClose_T canClose, canChangeBaudRate_T canChangeBaudrate, canEnumerate_T canEnumerate)
        {
            this.canReceive = canReceive;
            this.canSend = canSend;
            this.canOpen = canOpen;
            this.canClose = canClose;
            this.canChangeBaudrate = canChangeBaudrate;
            this.canEnumerate = canEnumerate;


            StringBuilder[] b = new StringBuilder[2];

            instancehandle = IntPtr.Zero;
            brdptr = IntPtr.Zero;

        }

        public static List<string> ports = new List<string>();

        public static void PrintReceivedData(string[] values, int valueCount)
        {
            foreach (var item in values)
                ports.Add(item);
        }


        public void enumerate()
        {
            ports = new List<string>();
            this.canEnumerate(PrintReceivedData);
        }

        /// <summary>
        /// Open the CAN device, the bus ID and bit rate are passed to driver. For Serial/USb Seral pass COMx etc.
        /// </summary>
        /// <param name="bus">The requested bus ID are provided here.</param>
        /// <param name="speed">The requested CAN bit rate</param>
        /// <returns>True on succesful opening of device</returns>
        public bool open(string bus, BUSSPEED speed)
        {

            try
            {


                brd.busname = bus;

                // Map BUSSPEED to CanFestival speed options
                switch (speed)
                {
                    case BUSSPEED.BUS_10Kbit:
                        brd.baudrate = "10K";
                        break;
                    case BUSSPEED.BUS_20Kbit:
                        brd.baudrate = "20K";
                        break;
                    case BUSSPEED.BUS_50Kbit:
                        brd.baudrate = "50K";
                        break;
                    case BUSSPEED.BUS_100Kbit:
                        brd.baudrate = "100K";
                        break;
                    case BUSSPEED.BUS_125Kbit:
                        brd.baudrate = "125K";
                        break;
                    case BUSSPEED.BUS_250Kbit:
                        brd.baudrate = "250K";
                        break;
                    case BUSSPEED.BUS_500Kbit:
                        brd.baudrate = "500K";
                        break;
                    case BUSSPEED.BUS_1Mbit:
                        brd.baudrate = "1M";
                        break;

                }

                brdptr = Marshal.AllocHGlobal(Marshal.SizeOf(brd));
                Marshal.StructureToPtr(brd, brdptr, false);

                instancehandle = canOpen(brdptr);

                if (instancehandle != IntPtr.Zero)
                {

                    rxthread = new System.Threading.Thread(rxthreadworker);
                    rxthread.Start();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// See if the CAN device is open
        /// </summary>
        /// <returns>Open status of can device</returns>
        public bool isOpen()
        {
            if (instancehandle == IntPtr.Zero)
                return false;


            return true;
        }

        /// <summary>
        /// Close the CAN hardware device
        /// </summary>
        public void close()
        {
            threadrun = false;

            System.Threading.Thread.Sleep(100);

            if(rxthread!=null)
            while(rxthread.ThreadState == System.Threading.ThreadState.Running)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (instancehandle != IntPtr.Zero)
                canClose(instancehandle);

            instancehandle = IntPtr.Zero;

            if (brdptr != IntPtr.Zero)
                Marshal.FreeHGlobal(brdptr);

            brdptr = IntPtr.Zero;
        }

        /// <summary>
        /// Message pump function. This should be called in a fast loop
        /// </summary>
        /// <returns></returns>
        public Message canreceive()
        {

            // I think we can do better here and not allocated/deallocate to heap every pump loop
            Message msg = new Message();

            IntPtr msgptr = Marshal.AllocHGlobal(Marshal.SizeOf(msg));
            Marshal.StructureToPtr(msg, msgptr, false);

            byte status = canReceive(instancehandle, msgptr);

            msg = (Message)Marshal.PtrToStructure(msgptr, typeof(Message));

            Marshal.FreeHGlobal(msgptr);

            return msg;

        }

        /// <summary>
        /// Send a CanOpen mesasge to the hardware device
        /// </summary>
        /// <param name="msg">CanOpen message to be sent</param>
        public void cansend(Message msg)
        {


            IntPtr msgptr = Marshal.AllocHGlobal(Marshal.SizeOf(msg));
            Marshal.StructureToPtr(msg, msgptr, false);

            if(instancehandle!=null)
               canSend(instancehandle, msgptr);

            Marshal.FreeHGlobal(msgptr);

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

                    DriverInstance.Message rxmsg = canreceive();

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
