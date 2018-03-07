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
using System.Windows.Shapes;

namespace SCYTChargeSystem
{
	/// <summary>
	/// Window1.xaml 的交互逻辑
	/// </summary>
	public partial class LoginWindow:Window
	{
		public LoginWindow()
		{
			InitializeComponent();
		}

		public List<string> Loginmessage
		{
			get;
			set;
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			//loginmessage.Key = Loginmessage[0];
		}

		private void Confirm_Click(object sender,RoutedEventArgs e)
		{
			//if(Account.Text = )
		}

		private void Password_KeyDown(object sender,KeyEventArgs e)
		{
			if(e.Key == Key.Enter)   //  if (e.KeyValue == 13) 判断是回车键
			{
				this.Confirm.Focus();
				Confirm_Click(sender,e);   //调用登录按钮的事件处理代码
			}
		}
	}
}
