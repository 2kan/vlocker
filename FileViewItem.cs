using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace vlocker
{
	public class FileViewItem : TreeViewItem
	{
		private string m_fullpath;

		public string FullPath
		{
			get { return m_fullpath; }
			set { m_fullpath = value; }
		}
	}
}
