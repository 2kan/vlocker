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
		public MainWindow ()
		{
			InitializeComponent();

			//LockerFactory.CreateLocker( "locker.vlocker" );
			Locker locker = new Locker( "locker.vlocker" );
			locker.LoadLocker();
			//a.FileDirectory.AddFile( "test.txt" );
			//byte[] file = a.FileDirectory.GetFile( "test.txt" );

			TreeViewItem treeItem = new TreeViewItem();
			treeItem.Header = "root";

			string[] filenames = locker.FileDirectory.GetFilenames();
			for ( int i = 0; i < filenames.Length; ++i )
				treeItem.Items.Add( new TreeViewItem() { Header = filenames[i] } );

			fileTree.Items.Add( treeItem );
		}
	}
}
