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
		private int m_length;
		private int m_offset;
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

		public int Length
		{
			get { return (int) m_length; }
		}

		public int Offset
		{
			get { return (int) m_offset; }
		}

		public File ( LockerConfig a_lockerConfig, string a_path, int a_length, int a_offset )
		{
			m_path = a_path;
			m_length = a_length;
			m_offset = a_offset;
			m_config = a_lockerConfig;
		}

		/// <summary>
		/// Load file entry from raw file directory data
		/// </summary>
		/// <param name="a_lockerConfig">LockerConfig object for this locker</param>
		/// <param name="a_entryData">Raw data of file entry</param>
		public File ( LockerConfig a_lockerConfig, byte[] a_entryData )
		{
			byte[] entryChecksum = a_entryData.Skip( a_entryData.Length - 20 ).Take( 20 ).ToArray();
			byte[] calculatedChecksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( entryChecksum );

			// TODO: manage invalid checksums better than this
			if ( !entryChecksum.SequenceEqual( calculatedChecksum ) )
				throw new Exception( "File entry corrupt." );

			m_path = Encoding.UTF8.GetString( a_entryData.Take( 4 * m_maxPathSize ).ToArray() ).TrimEnd( '\0' );
			m_offset = BitConverter.ToInt32( a_entryData.Skip( 4 * m_maxPathSize ).Take( sizeof( int ) ).ToArray(), 0 );
			m_length = BitConverter.ToInt32( a_entryData.Skip( 4 * m_maxPathSize + sizeof( int ) ).Take( sizeof( int ) ).ToArray(), 0 );
		}

		public byte[] GetData (int a_fileBlockOffset)
		{
			FileStream fs = new FileStream( m_config.Path, FileMode.Open );
			//int length = (int) fs.Length;
			byte[] bytes = new byte[m_length];

			fs.Seek( m_config.HeaderLength + a_fileBlockOffset + m_offset, SeekOrigin.Begin );
			fs.Read( bytes, 0, m_length );
			fs.Close();

			return bytes;
		}

		public byte[] GetEntry ()
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

		public static int GetEntrySize ()
		{
			return m_maxPathSize * 4 + sizeof( int ) * 2 + 20; // Currently 1068
		}
	}
}
