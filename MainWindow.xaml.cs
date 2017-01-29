using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vlocker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Locker m_locker;

		public MainWindow ()
		{
			InitializeComponent();

			//LockerFactory.CreateLocker( "locker.vlocker" );
			m_locker = new Locker( "locker.vlocker" );
			m_locker.LoadLocker();
			//a.FileDirectory.AddFile( "test.txt" );
			//byte[] file = a.FileDirectory.GetFile( "test.txt" );

			UpdateFileTree();
		}

		private void UpdateFileTree ()
		{
			TreeViewItem treeItem = new TreeViewItem();
			treeItem.Header = "root";

			string[] filenames = m_locker.FileDirectory.GetFilenames();
			for ( int i = 0; i < filenames.Length; ++i )
			{
				TreeViewItem file = new TreeViewItem() { Header = filenames[i] };
				file.MouseDoubleClick += fileTree_itemDoubleClick;

				treeItem.Items.Add( file );
			}

			treeItem.IsExpanded = true;

			fileTree.Items.Clear();
			fileTree.Items.Add( treeItem );
		}

		private void btnAddFile_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

			bool? result = dialog.ShowDialog();

			if ( result == true )
			{
				string filename = dialog.FileName;
				m_locker.FileDirectory.AddFile( filename );
				UpdateFileTree();
			}
		}

		private void fileTree_itemDoubleClick(object sender, RoutedEventArgs e)
		{
			output.Text = Encoding.UTF8.GetString(m_locker.FileDirectory.GetFile(((TreeViewItem) sender).Header.ToString()));
		}
	}
}
