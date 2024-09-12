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
using System.Reflection.Metadata;
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

	public static class DriverLoader
	{
		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

		/// <summary>
		/// Attempt to load the requested can festival driver or SocketCan and return a DriverInstance class
		/// </summary>
		/// <param name="fileName"> Name of the dynamic library to load, note do not append .dll or .so. Use 'SocketCan' to load SocketCan driver</param>
		/// <returns></returns>
		public static IDriverInstance LoadDriver(string fileName)
		{


			if (IsRunningOnMono())
			{
				fileName += ".so";
				return DriverLoaderMono.LoadDriver(fileName);
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && fileName == "SocketCan")
			{
				return new DriverInstanceSocketCan();
			}
			else
			{

				fileName += ".dll";
				return DriverLoaderWin.LoadDriver(fileName);
			}

		}
	}

	#region windows

	/// <summary>
	/// CanFestival driver loader for windows, this class will load kernel32 then attept to use LoadLibrary()
	/// and GetProcAddress() to hook the can festival driver functions these are then exposed as delagates
	/// for eash C# access
	/// </summary>

	public static class DriverLoaderWin
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string libname);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		private static IntPtr Handle = IntPtr.Zero;

		private static DriverInstanceCanFestival driver;

		/// <summary>
		/// Attempt to load the requested can festival driver and return a DriverInstance class
		/// Only one CanFestival driver can be loaded in the app. If you need to load multiple drivers
		/// you will need to keep track of the handles and free them when done.
		/// </summary>
		/// <param name="fileName">Load can festival driver (Windows .Net runtime version) .dll must be appeneded in this case to fileName</param>
		/// <returns></returns>
		public static DriverInstanceCanFestival LoadDriver(string fileName)
		{
			// Load the library just once. Not bothering to free it. as it will get unloaded when the app closes.
			// If you need to load and unload multiple times then you will need to add a FreeLibrary() call
			if(Handle == IntPtr.Zero)
			{
				Handle = LoadLibrary(fileName);
			}

			if (Handle == IntPtr.Zero)
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new Exception(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
			}

			IntPtr funcaddr;

			funcaddr = GetProcAddress(Handle, "canReceive_driver");
			DriverInstanceCanFestival.canReceive_T canReceive = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canReceive_T)) as DriverInstanceCanFestival.canReceive_T;

			funcaddr = GetProcAddress(Handle, "canSend_driver");
			DriverInstanceCanFestival.canSend_T canSend = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canSend_T)) as DriverInstanceCanFestival.canSend_T; ;

			funcaddr = GetProcAddress(Handle, "canOpen_driver");
			DriverInstanceCanFestival.canOpen_T canOpen = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canOpen_T)) as DriverInstanceCanFestival.canOpen_T; ;

			funcaddr = GetProcAddress(Handle, "canClose_driver");
			DriverInstanceCanFestival.canClose_T canClose = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canClose_T)) as DriverInstanceCanFestival.canClose_T; ;

			funcaddr = GetProcAddress(Handle, "canChangeBaudRate_driver");
			DriverInstanceCanFestival.canChangeBaudRate_T canChangeBaudRate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canChangeBaudRate_T)) as DriverInstanceCanFestival.canChangeBaudRate_T; ;

			funcaddr = GetProcAddress(Handle, "canEnumerate2_driver");
			DriverInstanceCanFestival.canEnumerate_T canEnumerate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canEnumerate_T)) as DriverInstanceCanFestival.canEnumerate_T; ;

			driver = new DriverInstanceCanFestival(canReceive, canSend, canOpen, canClose, canChangeBaudRate, canEnumerate);

			return driver;
		}

	}
	#endregion

	#region mono

	/// <summary>
	/// CanFestival driver loader for mono, this class will load libdl then attempt to use dlopen() and dlsym()
	/// and GetProcAddress to hook the can festival driver functions these are then exposed as delagates
	/// for each C# access
	/// 
	/// Only one CanFestival driver can be loaded in the app. If you need to load multiple drivers
	/// you will need to keep track of the handles and free them when done.
	/// </summary>
	/// 
	public static class DriverLoaderMono
	{

		[DllImport("libdl.so")]
		static extern IntPtr dlopen(string filename, int flags);

		[DllImport("libdl.so")]
		static extern IntPtr dlsym(IntPtr handle, string symbol);

		private static IntPtr Handle = IntPtr.Zero;

		private static DriverInstanceCanFestival driver;

		const int RTLD_NOW = 2; // for dlopen's flags 

		/// <summary>
		/// Attempt to load the requested can festival driver and return a DriverInstance class
		/// </summary>
		/// <param name="fileName">Load can festival driver (Mono runtime version) .so must be appeneded in this case to fileName</param>
		/// <returns></returns>
		public static DriverInstanceCanFestival LoadDriver(string fileName)
		{
			// Load the library just once. Not bothering to free it. as it will get unloaded when the app closes.
			// If you need to load and unload multiple times then you will need to add a FreeLibrary() call
			if (Handle == IntPtr.Zero)
			{
				Handle = dlopen(fileName, RTLD_NOW);
			}
			
			if (Handle == IntPtr.Zero)
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new Exception(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
			}

			IntPtr funcaddr;

			funcaddr = dlsym(Handle, "canReceive_driver");
			DriverInstanceCanFestival.canReceive_T canReceive = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canReceive_T)) as DriverInstanceCanFestival.canReceive_T;

			funcaddr = dlsym(Handle, "canSend_driver");
			DriverInstanceCanFestival.canSend_T canSend = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canSend_T)) as DriverInstanceCanFestival.canSend_T; ;

			funcaddr = dlsym(Handle, "canOpen_driver");
			DriverInstanceCanFestival.canOpen_T canOpen = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canOpen_T)) as DriverInstanceCanFestival.canOpen_T; ;

			funcaddr = dlsym(Handle, "canClose_driver");
			DriverInstanceCanFestival.canClose_T canClose = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canClose_T)) as DriverInstanceCanFestival.canClose_T; ;

			funcaddr = dlsym(Handle, "canChangeBaudRate_driver");
			DriverInstanceCanFestival.canChangeBaudRate_T canChangeBaudRate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canChangeBaudRate_T)) as DriverInstanceCanFestival.canChangeBaudRate_T; ;

			funcaddr = dlsym(Handle, "canEnumerate_driver");
			DriverInstanceCanFestival.canEnumerate_T canEnumerate = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DriverInstanceCanFestival.canEnumerate_T)) as DriverInstanceCanFestival.canEnumerate_T; ;

			driver = new DriverInstanceCanFestival(canReceive, canSend, canOpen, canClose, canChangeBaudRate, canEnumerate);

			return driver;
		}
	}

#endregion
}
