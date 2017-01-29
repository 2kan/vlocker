﻿using System;
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
			Locker a = new Locker( "locker.vlocker" );
			a.LoadLocker();
			a.FileDirectory.AddFile( "test.txt" );

			byte[] file = a.FileDirectory.GetFile( "test.txt" );
		}
	}
}