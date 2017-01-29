using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public class FileDirectory
	{
		private List<File> m_directory;
		private LockerConfig m_config;

		public FileDirectory (LockerConfig a_config)
		{
			m_directory = new List<File>();
			m_config = a_config;
		}

		public byte[] GetBlock ()
		{
			// Block length is the total length of all file pointers stored,
			// plus 4 bytes (int) for the number of files that are stored,
			// plus 20 bytes for the sha1 checksum
			byte[] block = new byte[sizeof(int) + m_directory.Count * File.GetEntrySize() + 20];
			
			// Add number of files to block
			byte[] countBytes = BitConverter.GetBytes( m_directory.Count );
			for ( int i = 0; i < countBytes.Length; ++i )
				block[i] = countBytes[i];

			// Get the bytes for each file pointer and add them to the block
			for ( int i = 0; i < m_directory.Count; ++i )
			{
				byte[] entry = m_directory[i].GetEntry();
				for (int k = 0; k<entry.Length; ++k )
				{
					block[i * File.GetEntrySize() + k] = entry[k];
				}
			}

			// Calculate hash of whole block and append
			byte[] checksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( block );
			for ( int i = 0; i < checksum.Length; ++i )
				block[sizeof( int ) + m_directory.Count * File.GetEntrySize() + i] = checksum[i];

			return block;
		}
	}
}
