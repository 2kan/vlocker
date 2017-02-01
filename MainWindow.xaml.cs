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
		private ContextMenu m_defaultMenu;

		public MainWindow ()
		{
			InitializeComponent();

			//LockerFactory.CreateLocker( "locker.vlocker" );
			m_locker = new Locker( "locker.vlocker" );
			m_locker.LoadLocker();
			//m_locker.FileDirectory.AddFile( "test.txt" );
			//byte[] file = a.FileDirectory.GetFile( "test.txt" );

			m_defaultMenu = new ContextMenu();
			var a = new MenuItem();
			a.Header = "Delete";
			a.Click += DeleteNode;
			m_defaultMenu.Items.Add( a );

			UpdateFileTree();
		}

		private void UpdateFileTree ()
		{
			TreeViewItem treeItem = new TreeViewItem();
			treeItem.Header = "root";

			Dictionary<string, TreeViewItem> map = new Dictionary<string, TreeViewItem>();
			map.Add( treeItem.Header.ToString(), treeItem );

			File[] files = m_locker.FileDirectory.GetFiles();
			for ( int i = 0; i < files.Length; ++i )
			{
				FileViewItem file = new FileViewItem()
				{
					Header = files[i].Filename,
					FullPath = files[i].FullPath,
					ContextMenu = m_defaultMenu
				};
				file.MouseDoubleClick += fileTree_itemDoubleClick;
				file.MouseRightButtonUp += ( object a_sender, MouseButtonEventArgs a_args ) =>
				{
					( (FileViewItem) a_sender ).IsSelected = true;
				};

				if ( map.ContainsKey( files[i].Path ) )
					map[files[i].Path].Items.Add( file );
				else
				{
					AddFolderNode( map, files[i].Path ).Items.Add( file );

				}
			}

			//treeItem.Items.Add( map.First().Value );
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
				string filename = dialog.SafeFileName;
				m_locker.FileDirectory.AddFile( filename );
				UpdateFileTree();
			}
		}

		private void fileTree_itemDoubleClick ( object sender, RoutedEventArgs e )
		{
			output.Text = Encoding.UTF8.GetString( m_locker.FileDirectory.GetFile( ( (FileViewItem) sender ).FullPath ) );
		}

		private TreeViewItem AddFolderNode ( Dictionary<string, TreeViewItem> a_map, string a_path )
		{
			if ( a_map.ContainsKey( a_path ) )
				return a_map[a_path];

			string[] tokens = a_path.Split( '\\' );
			TreeViewItem thisItem = new TreeViewItem() { Header = tokens[tokens.Length - 1] };

			for ( int i = a_path.Length - 1; i >= 0; --i )
			{
				if ( a_path[i] == '\\' )
				{
					AddFolderNode( a_map, a_path.Substring( 0, i ) );
					a_map[a_path.Substring( 0, i )].Items.Add( thisItem );
					break;
				}
			}

			a_map.Add( a_path, thisItem );

			return a_map[a_path];
		}

		private void AddFolder ( object a_sender, RoutedEventArgs a_args )
		{
			// TODO: this
			//m_locker.FileDirectory.AddFolder("New Folder");
		}

		private void Rename ( object a_sender, RoutedEventArgs a_args )
		{
			// TODO: this
		}

		private void DeleteNode ( object a_sender, RoutedEventArgs a_args )
		{
			FileViewItem fileNode = (FileViewItem) ( ( (ContextMenu) ( (MenuItem) a_sender ).Parent ).PlacementTarget );
			var node = m_locker.FileDirectory.GetFile( fileNode.FullPath );
		}
	}
}
