using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vlocker
{
	public partial class MainForm : Form
	{
		public MainForm ()
		{
			InitializeComponent();

			//LockerFactory.CreateLocker( "locker.vlocker" );
			Locker a = new Locker( "locker.vlocker" );
			a.LoadLocker();
			a.FileDirectory.AddFile( "test.txt" );

			byte[] file = a.FileDirectory.GetFile( "test.txt" );
		}
	}
}
