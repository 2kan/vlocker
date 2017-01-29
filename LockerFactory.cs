using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public static class LockerFactory
	{
		public static Locker CreateLocker ( string a_path, string a_name = "Unnamed Locker", bool a_encoded = true )
		{
			if (a_path.IndexOf(":\\") == -1)
			{
				a_path = Environment.CurrentDirectory + "\\" + a_path;
			}

			Locker locker = new Locker( a_path, a_name, a_encoded );
			locker.CreateLocker();

			return locker;
		}
	}
}
