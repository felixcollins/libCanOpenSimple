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
		event DriverInstanceCanFestival.RxMessage rxmessage;

		DriverInstanceCanFestival.Message canreceive();
		void cansend(DriverInstanceCanFestival.Message msg);
		void close();
		void enumerate();
		bool isOpen();
		bool open(string bus, BUSSPEED speed);
	}
}