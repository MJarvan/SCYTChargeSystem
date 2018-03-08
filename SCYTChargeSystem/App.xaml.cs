using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SCYTChargeSystem
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App:Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			List<string> list = e.Args.ToList();
			LoginWindow login = new LoginWindow();
			login.Loginmessage = list;
			MainWindow main = new MainWindow();
			main.ShowDialog();
			Shutdown();
		}
	}
}
