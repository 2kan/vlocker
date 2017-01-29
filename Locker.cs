using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public class Locker
	{
		private LockerConfig m_config;
		private FileDirectory m_fileDirectory;

		public Locker ( string a_path, string a_name = "Unnamed Locker", bool a_encoded = true )
		{
			m_config = new LockerConfig( a_path, a_name, a_encoded );
			m_fileDirectory = new FileDirectory(m_config);
		}

		public bool CreateLocker ()
		{
			IEnumerable<byte> data = Enumerable.Empty<byte>();
			data = data.Concat( m_config.GetHeader() );
			data = data.Concat( m_fileDirectory.GetBlock() );
			byte[] raw = data.ToArray();

			FileStream fs = new FileStream(m_config.Path, FileMode.Create);
			fs.Write( raw, 0, raw.Length );
			fs.Close();

			return true;
		}
	}
}
