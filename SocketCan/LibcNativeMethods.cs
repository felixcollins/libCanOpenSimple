#region License
/* 
BSD 3-Clause License

Copyright (c) 2021, Derek Will
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace libCanOpenSimple.SocketCan
{
    /// <summary>
    /// C Standard Library Native Methods
    /// </summary>
    public static class LibcNativeMethods
    {
        /// <summary>
        /// Number of the last error which indicates what went wrong. Set by system calls and some library functions when an error occurs.
        /// </summary>
        public static int Errno { get { return Marshal.GetLastWin32Error(); } }

        /// <summary>
        /// Creates a CAN socket.
        /// </summary>
        /// <param name="addressFamily">Address Family</param>
        /// <param name="socketType">Type of socket</param>
        /// <param name="protocolType">CAN Protocol Type</param>
        /// <returns>Socket Handle Wrapper Instance</returns>
        [DllImport("libc", EntryPoint = "socket", SetLastError = true)]
        public static extern SafeFileDescriptorHandle Socket(int addressFamily, SocketType socketType, SocketCanProtocolType protocolType);

        /// <summary>
        /// Manipulates the underlying device parameters of special files.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="request">Request Code</param>
        /// <param name="arg">Integer Argument</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int Ioctl(SafeFileDescriptorHandle socketHandle, int request, ref int arg);

        /// <summary>
        /// Manipulates the underlying device parameters of special files.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="request">Request Code</param>
        /// <param name="ifreq">Interface Request structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int Ioctl(SafeFileDescriptorHandle socketHandle, int request, [In][Out] Ifreq ifreq);

        /// <summary>
        /// Manipulates the underlying device parameters of special files.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="request">Request Code</param>
        /// <param name="ifreq">Interface Request structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int Ioctl(SafeFileDescriptorHandle socketHandle, int request, [In][Out] IfreqMtu ifreq);

        /// <summary>
        /// Assigns the specified SocketCAN base address to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN base address structure</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "bind", SetLastError = true)]
        public static extern int Bind(SafeFileDescriptorHandle socketHandle, SockAddrCan addr, int addrSize);


        /// <summary>
        /// Establishes a connection on the socket to the specified SocketCAN base address.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN base address structure containing the peer address</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "connect", SetLastError = true)]
        public static extern int Connect(SafeFileDescriptorHandle socketHandle, SockAddrCan addr, int addrSize);

        /// <summary>
        /// Write the CanFrame to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrappper Instance</param>
        /// <param name="frame">CAN Frame to write</param>
        /// <param name="frameSize">Size of CAN Frame in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "write", SetLastError = true)]
        public static extern int Write(SafeFileDescriptorHandle socketHandle, ref Message frame, int frameSize);


        /// <summary>
        /// Write the byte array to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte Array to write</param>
        /// <param name="dataSize">Size of Byte Array</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "write", SetLastError = true)]
        public static extern int Write(SafeFileDescriptorHandle socketHandle, byte[] data, int dataSize);

        /// <summary>
        /// Read a CanFrame from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN Frame structure to populate</param>
        /// <param name="frameSize">Size of CAN Frame structure</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "read", SetLastError = true)]
        public static extern int Read(SafeFileDescriptorHandle socketHandle, ref Message frame, int frameSize);


        /// <summary>
        /// Read a byte array from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array to populate</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "read", SetLastError = true)]
        public static extern int Read(SafeFileDescriptorHandle socketHandle, [Out] byte[] data, int dataSize);


        /// <summary>
        /// Returns a pointer to an array of IfNameIndex objects. Each IfNameIndex object includes information about one of the network interfaces on the local system.
        /// </summary>
        /// <returns>Pointer to an array of IfNameIndex objects</returns>
        [DllImport("libc", EntryPoint = "if_nameindex", SetLastError = true)]
        public static extern nint IfNameIndex();

        /// <summary>
        /// Frees the dynamically allocated data structure returned by IfNameIndex().
        /// </summary>
        /// <param name="ptr">Pointer to an array of IfNameIndex objects</param>
        [DllImport("libc", EntryPoint = "if_freenameindex", SetLastError = true)]
        public static extern void IfFreeNameIndex(nint ptr);

        /// <summary>
        /// Closes a file descriptor.
        /// </summary>
        /// <param name="fd">File descriptor to close.</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "close", SetLastError = true)]
        public static extern int Close(nint fd);

        /// <summary>
        /// Returns the current address to which the socket is bound to.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="sockAddr">Address structure</param>
        /// <param name="sockAddrLen">The size of the the socket address structure in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "getsockname", SetLastError = true)]
        public static extern int GetSockName(SafeFileDescriptorHandle socketHandle, SockAddrCan sockAddr, ref int sockAddrLen);

        /// <summary>
        /// Retrieves the index of the network interface corresponding to the specified name.
        /// </summary>
        /// <param name="name">Interface Name</param>
        /// <returns>Interface Index on success, 0 on failure</returns>
        [DllImport("libc", EntryPoint = "if_nametoindex", SetLastError = true)]
        public static extern uint IfNameToIndex(string name);

        /// <summary>
        /// Retrieves the name of the network interface corresponding to the specified index.
        /// </summary>
        /// <param name="index">Interface Index</param>
        /// <param name="namePtr">Pointer to the buffer where the Interface Name is set</param>
        /// <returns>Valid IntPtr pointing to a buffer containing the Interface Name on success, IntPtr.Zero (null) on failure.</returns>
        [DllImport("libc", EntryPoint = "if_indextoname", SetLastError = true)]
        public static extern nint IfIndexToName(uint index, nint namePtr);
    }
}