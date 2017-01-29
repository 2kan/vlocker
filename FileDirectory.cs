using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace vlocker
{
	public class FileDirectory
	{
		private static int m_maxBlockPoolFiles = 100;
		private static int m_blockPoolLength = File.GetEntrySize() * m_maxBlockPoolFiles;
		private List<File> m_directory;
		private LockerConfig m_config;

		public FileDirectory ( LockerConfig a_config )
		{
			m_directory = new List<File>();
			m_config = a_config;
		}

		public byte[] GetBlock ()
		{
			// Block length is the total length of a file pointers * block pool length,
			// plus 4 bytes (int) for the number of files that are stored,
			// plus 20 bytes for the sha1 checksum
			byte[] block = new byte[
				m_blockPoolLength *
				(int) ( Math.Ceiling( (double) ( m_directory.Count / m_maxBlockPoolFiles ) + 1 ) ) +
				sizeof( int ) +
				20];//sizeof( int ) + m_directory.Count * File.GetEntrySize() + 20];

			// Add number of files to block
			byte[] countBytes = BitConverter.GetBytes( m_directory.Count );
			for ( int i = 0; i < countBytes.Length; ++i )
				block[i] = countBytes[i];

			// Get the bytes for each file pointer and add them to the block
			for ( int i = 0; i < m_directory.Count; ++i )
			{
				byte[] entry = m_directory[i].GetEntry();
				for ( int k = 0; k < entry.Length; ++k )
				{
					block[i * File.GetEntrySize() + k] = entry[k];
				}
			}

			// Calculate hash of whole block and append
			byte[] checksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( block.Take( block.Length - 20 ).ToArray() );
			for ( int i = 0; i < checksum.Length; ++i )
				block[( block.Length - 20 ) + i] = checksum[i];

			return block;
		}

		public int LoadDirectory ( byte[] a_data )
		{
			int numFiles = BitConverter.ToInt32( a_data.Take( sizeof( int ) ).ToArray(), 0 );

			int blockLength = m_blockPoolLength *
				(int) ( Math.Ceiling( (double) ( m_directory.Count / m_maxBlockPoolFiles ) + 1 ) ) +
				sizeof( int ) +
				20;//sizeof( int ) + numFiles * File.GetEntrySize() + 20;

			// Add each file to the directory
			for ( int i = 0; i < numFiles; ++i )
			{
				m_directory.Add( new File( m_config, a_data.Skip( sizeof(int) + i * File.GetEntrySize() ).Take( File.GetEntrySize() ).ToArray() ) );
			}

			byte[] blockChecksum = a_data.Skip( blockLength - 20 ).Take( 20 ).ToArray();
			var preChecksum = BitConverter.GetBytes(numFiles).Concat(GetBlock().Skip(sizeof(int)).Take(blockLength - sizeof(int) - 20)).ToArray();
			byte[] calculatedChecksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( preChecksum );

			if ( !blockChecksum.SequenceEqual( calculatedChecksum ) )
				return -1;

			return blockLength;
		}

		public bool AddFile ( string a_path )
		{
			// TODO: add virtual paths
			string[] tokens = a_path.Split( '/' );
			string filename = tokens[tokens.Length - 1];

			byte[] bytes = System.IO.File.ReadAllBytes( a_path );

			int endOffset = 0; // Offset for the end of the last file,
							   // though it can probably be made to be the size of the locker file,
							   // minus some checksum length

			for ( int i = 0; i < m_directory.Count; ++i )
			{
				int thisEnd = m_directory[i].Offset + m_directory[i].Length;

				if ( thisEnd > endOffset )
					endOffset = thisEnd;
			}

			int curBlockSize = GetBlock().Length;
			m_directory.Add( new File( m_config, filename, bytes.Length, endOffset ) );

			FileStream fs = new FileStream( m_config.Path, FileMode.Open );

			// Write file entry to directory
			byte[] entryBytes = m_directory[m_directory.Count - 1].GetEntry();
			fs.Seek( m_config.GetHeader().Length + ( m_directory.Count - 1 ) * File.GetEntrySize() + sizeof( int ), SeekOrigin.Begin );
			fs.Write( entryBytes, 0, entryBytes.Length );

			// Update file count
			byte[] countBytes = BitConverter.GetBytes( m_directory.Count );
			fs.Seek( m_config.GetHeader().Length, SeekOrigin.Begin );
			fs.Write( countBytes, 0, countBytes.Length );

			// Update checksum
			byte[] block = GetBlock();
			byte[] checksum = ( new SHA1CryptoServiceProvider() ).ComputeHash( block.Take( block.Length - 20 ).ToArray(); );
			fs.Seek( m_config.GetHeader().Length + block.Length - 20, SeekOrigin.Begin );
			fs.Write( checksum, 0, checksum.Length );


			// Write file data after header and directory
			fs.Seek( curBlockSize + m_config.GetHeader().Length + endOffset, SeekOrigin.Begin );
			fs.Write( bytes, 0, bytes.Length );
			fs.Close();

			return true;
		}

		public byte[] GetFile ( string a_filename )
		{
			for ( int i = 0; i < m_directory.Count; ++i )
			{
				if ( m_directory[i].Filename == a_filename )
					return m_directory[i].GetData( GetBlock().Length );
			}

			return null;
		}

		public string[] GetFilenames ()
		{
			string[] filenames = new string[m_directory.Count];
			for ( int i = 0; i < m_directory.Count; ++i )
				filenames[i] = m_directory[i].Filename;

			return filenames;
		}
	}
}
