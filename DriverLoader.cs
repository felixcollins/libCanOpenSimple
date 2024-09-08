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
using System.Runtime.InteropServices;

namespace libCanOpenSimple
{
	/// <summary> DriverLoader - dynamic pinvoke can festival drivers
	/// This class will select the approprate win or mono loader and try to load the requested 
	/// can festival library
	/// Info on pinvoke for win/mono :-
	/// http://stackoverflow.com/questions/13461989/p-invoke-to-dynamically-loaded-library-on-mono
	/// by gordonmleigh
	/// </summary>

	public class DriverLoader
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Attempt to load the requested can festival driver and return a DriverInstance class
        /// </summary>
        /// <param name="fileName"> Name of the dynamic library to load, note do not append .dll or .so</param>
        /// <returns></returns>
        public DriverInstance loaddriver(string fileName)
        {
          

            if (IsRunningOnMono())
            {
                fileName += ".so";
                DriverLoaderMono dl = new DriverLoaderMono();
                return dl.loaddriver(fileName);
            }
            else
            {
              
                fileName += ".dll";
                DriverLoaderWin dl = new DriverLoaderWin();
                return dl.loaddriver(fileName);
            }

        }
    }

    #region windows

    /// <summary>
    /// CanFestival driver loader for windows, this class will load kernel32 then attept to use LoadLibrary()
    /// and GetProcAddress() to hook the can festival driver functions these are then exposed as delagates
    /// for eash C# access
    /// </summary>

    public class DriverLoaderWin
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private IntPtr Handle = IntPtr.Zero;

        DriverInstance driver;

        /// <summary>
        /// Clean up and free the library
        /// </summary>
        ~DriverLoaderWin()
        {
            if (Handle != IntPtr.Zero)
            {
                FreeLibrary(Handle);
                Handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Attempt to load the requested can festival driver and return a DriverInstance class
        /// </summary>
        /// <param name="fileName">Load can festival driver (Windows .Net runtime version) .dll must be appeneded in this case to fileName</param>
        /// <returns></returns>
        public DriverInstance loaddriver(string fileName)
        {

            IntPtr Handle = LoadLibrary(fileName);

            if (Handle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
            }

            IntPtr funcaddr;

            funcaddr = GetProcAddress(Handle, "canReceive_driver");
            DriverInstance.canReceive_T canReceive = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canReceive_T)) as DriverInstance.canReceive_T;

            funcaddr = GetProcAddress(Handle, "canSend_driver");
            DriverInstance.canSend_T canSend = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canSend_T)) as DriverInstance.canSend_T; ;

            funcaddr = GetProcAddress(Handle, "canOpen_driver");
            DriverInstance.canOpen_T canOpen = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canOpen_T)) as DriverInstance.canOpen_T; ;

            funcaddr = GetProcAddress(Handle, "canClose_driver");
            DriverInstance.canClose_T canClose = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canClose_T)) as DriverInstance.canClose_T; ;

            funcaddr = GetProcAddress(Handle, "canChangeBaudRate_driver");
            DriverInstance.canChangeBaudRate_T canChangeBaudRate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canChangeBaudRate_T)) as DriverInstance.canChangeBaudRate_T; ;

            funcaddr = GetProcAddress(Handle, "canEnumerate2_driver");
            DriverInstance.canEnumerate_T canEnumerate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canEnumerate_T)) as DriverInstance.canEnumerate_T; ;

            driver = new DriverInstance(canReceive, canSend, canOpen, canClose, canChangeBaudRate,canEnumerate);

            return driver;
        }

    }
    #endregion

    #region mono

    /// <summary>
    /// CanFestival driver loader for mono, this class will load libdl then attept to use dlopen() and dlsym()
    /// and GetProcAddress to hook the can festival driver functions these are then exposed as delagates
    /// for eash C# access
    /// </summary>
    /// 
    public class DriverLoaderMono
    {

        [DllImport("libdl.so")]
        protected static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);

        DriverInstance driver;

        const int RTLD_NOW = 2; // for dlopen's flags 

        /// <summary>
        /// Attempt to load the requested can festival driver and return a DriverInstance class
        /// </summary>
        /// <param name="fileName">Load can festival driver (Mono runtime version) .so must be appeneded in this case to fileName</param>
        /// <returns></returns>
        public DriverInstance loaddriver(string fileName)
        {
            IntPtr Handle = dlopen(fileName, RTLD_NOW);
            if (Handle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
            }

            IntPtr funcaddr;

            funcaddr = dlsym(Handle, "canReceive_driver");
            DriverInstance.canReceive_T canReceive = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canReceive_T)) as DriverInstance.canReceive_T;

            funcaddr = dlsym(Handle, "canSend_driver");
            DriverInstance.canSend_T canSend = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canSend_T)) as DriverInstance.canSend_T; ;

            funcaddr = dlsym(Handle, "canOpen_driver");
            DriverInstance.canOpen_T canOpen = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canOpen_T)) as DriverInstance.canOpen_T; ;

            funcaddr = dlsym(Handle, "canClose_driver");
            DriverInstance.canClose_T canClose = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canClose_T)) as DriverInstance.canClose_T; ;

            funcaddr = dlsym(Handle, "canChangeBaudRate_driver");
            DriverInstance.canChangeBaudRate_T canChangeBaudRate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canChangeBaudRate_T)) as DriverInstance.canChangeBaudRate_T; ;

            funcaddr = dlsym(Handle, "canEnumerate_driver");
            DriverInstance.canEnumerate_T canEnumerate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstance.canEnumerate_T)) as DriverInstance.canEnumerate_T; ;

            driver = new DriverInstance(canReceive, canSend, canOpen, canClose, canChangeBaudRate,canEnumerate);



            return driver;
        }
    }

#endregion
}
