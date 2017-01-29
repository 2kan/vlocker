using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public class LockerConfig
	{
		private string m_path;
		private string m_lockerName;
		private bool m_encoded;

		private int m_maxNameLength = 255;
		private int m_configVersion = 1;

		public string Path
		{
			get { return m_path; }
			set { m_path = value; }
		}
		public string Name
		{
			get { return m_lockerName; }
			set { m_lockerName = value; } // TODO: clamp to size
		}
		public bool Encoded
		{
			get { return m_encoded; }
			set { m_encoded = value; }
		}

		public int HeaderLength
		{
			get { return GetHeaderLength(); }
		}

		public LockerConfig ( string a_path )
		{
			m_path = a_path;
		}

		public byte[] GetHeader ()
		{
			// Create header byte array
			byte[] header = new byte[GetHeaderLength()];

			// Add version number to start of header
			byte[] version = BitConverter.GetBytes( m_configVersion );
			for ( int i = 0; i < version.Length; ++i )
				header[i] = version[i];

			// Add locker name
			byte[] name = Encoding.UTF8.GetBytes( m_lockerName );
			for ( int i = 0; i < name.Length; ++i )
				header[version.Length + i] = name[i];

			// Calculate hash of whole header (minus 20 bytes for hash) and append
			byte[] checksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( header.Take( GetHeaderLength() - 20 ).ToArray() );
			for ( int i = 0; i < checksum.Length; ++i )
				header[version.Length + m_maxNameLength * 4 + i] = checksum[i];

			return header;
		}

		public int LoadHeader ( byte[] a_data )
		{
			int headerLength = GetHeaderLength();

			byte[] headerBlock = a_data.Take( headerLength - 20 ).ToArray();
			byte[] headerChecksum = a_data.Skip( headerLength - 20 ).Take( 20 ).ToArray();
			byte[] calculatedChecksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( headerBlock );

			if ( !headerChecksum.SequenceEqual( calculatedChecksum ) )
				return -1;

			m_configVersion = BitConverter.ToInt32( headerBlock.Take( sizeof( int ) ).ToArray(), 0 );
			m_lockerName = Encoding.UTF8.GetString( headerBlock.Skip( sizeof( int ) ).Take( 4 * m_maxNameLength ).ToArray() ).TrimEnd('\0');

			return headerLength;
		}

		private int GetHeaderLength ()
		{
			// Length is equal to the max length of the name (4 bytes per UTF8 char),
			// plus 4 bytes for the version number, plus 20 bytes for the sha1 checksum
			// ( sha1 length is always 160 bits)
			return 4 * m_maxNameLength + sizeof( int ) + 20;
		}
	}
}
