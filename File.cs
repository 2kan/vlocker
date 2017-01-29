using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public class File
	{
		private static int m_maxPathSize = 260;
		private string m_path;
		private uint m_length;
		private uint m_offset;
		private LockerConfig m_config;

		public string Path
		{
			get { return m_path; }
		}

		public string Filename
		{
			get
			{
				string[] tokens = m_path.Split( '/' );
				return tokens[tokens.Length - 1];
			}
		}

		public File ( LockerConfig a_lockerConfig, string a_path, uint a_length, uint a_offset )
		{
			m_path = a_path;
			m_length = a_length;
			m_offset = a_offset;
			m_config = a_lockerConfig;
		}

		public byte[] GetData()
		{
			FileStream fs = new FileStream(m_config.Path, FileMode.Open);
			int length = (int) fs.Length;
			byte[] bytes = new byte[length];

			fs.Read( bytes, (int) (m_config.HeaderLength + m_offset), (int) m_length );
			fs.Close();

			return bytes;
		}

		public byte[] GetEntry()
		{
			// Length is the max path size (multiplied by 4 for UTF8), plus the size
			// of the file length and offset, plus 20 chars for sha1 checksum
			byte[] entry = new byte[GetEntrySize()];

			// Add the path to the block
			byte[] path = Encoding.UTF8.GetBytes( m_path );
			for ( int i = 0; i < path.Length; ++i )
				entry[i] = path[i];

			// Add offset value to the block
			byte[] offset = BitConverter.GetBytes( m_offset );
			for ( int i = 0; i < offset.Length; ++i )
				entry[m_maxPathSize * 4 + i] = offset[i];

			// Add file length to the block
			byte[] length = BitConverter.GetBytes( m_length );
			for ( int i = 0; i < length.Length; ++i )
				entry[m_maxPathSize * 4 + offset.Length + i] = length[i];
			
			// Calculate hash of whole block and append
			byte[] checksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( entry );
			for ( int i = 0; i < checksum.Length; ++i )
				entry[m_maxPathSize * 4 + offset.Length + length.Length + i] = checksum[i];

			return entry;
		}

		public static int GetEntrySize()
		{
			return m_maxPathSize * 4 + sizeof( uint ) * 2 + 20;
		}
	}
}
