using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCanOpenSimple
{
	class DriverInstanceSocketCan : IDriverInstance
	{
		public event DriverInstanceCanFestival.RxMessage rxmessage;

		public DriverInstanceCanFestival.Message canreceive()
		{
			throw new NotImplementedException();
		}

		public void cansend(DriverInstanceCanFestival.Message msg)
		{
			throw new NotImplementedException();
		}

		public void close()
		{
			throw new NotImplementedException();
		}

		public void enumerate()
		{
			throw new NotImplementedException();
		}

		public bool isOpen()
		{
			throw new NotImplementedException();
		}

		public bool open(string bus, BUSSPEED speed)
		{
			throw new NotImplementedException();
		}
	}
}
