using System;
using System.Collections.Generic;
using System.IO;
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

namespace SCYTChargeSystem
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow:Window
	{

		/// <summary>
		/// 应用程序路径
		/// </summary>
		string systempath = System.Windows.Forms.Application.StartupPath;

		/// <summary>
		/// 逻辑信息
		/// </summary>
		List<string> LogicInfo = new List<string>();

		/// <summary>
		/// 逻辑路径
		/// </summary>
		private static string LogicPath;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			LoadLogic();
		}

		/// <summary>
		/// 加载逻辑
		/// </summary>
		private void LoadLogic()
		{
			LogicInfo.Clear();

			LogicPath = systempath + "\\Logic.txt";
			if(File.Exists(LogicPath))
			{
				LogicInfo = File.ReadAllLines(LogicPath).ToList();
				bool errorState = false;

				if(LogicInfo.Count >= 3)
				{
					LogicCostMoneyTextbox.Text = LogicInfo[0];
					LogicTotalMoneyTextbox.Text = LogicInfo[1];
					LogicTotalMoneyReadTextbox.Text = LogicTotalMoneyTextbox.Text;
					LogicTicketNoTextbox.Text = LogicInfo[2];
					errorState = true;
				}

				if(!errorState)
				{
					MessageBox.Show("加载配置失败，已经为您调为默认配置");
				}
			}
			else
			{
				MessageBox.Show("加载配置失败，已经为您调为默认配置");
				LogicCostMoneyTextbox.Text = "100";
				LogicTotalMoneyTextbox.Text = "2000";
				LogicTotalMoneyReadTextbox.Text = LogicTotalMoneyTextbox.Text;
				LogicTicketNoTextbox.Text = "A100";
				LogicInfo.Add("100");
				LogicInfo.Add("2000");
				LogicInfo.Add("A100");
				LogicSave_Click(null,new RoutedEventArgs());
			}
		}

		/// <summary>
		/// 保存逻辑
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LogicSave_Click(object sender,RoutedEventArgs e)
		{
			if(sender == null)
			{
				if(LogicInfo == null)
				{
					return;
				}
				else
				{
					File.WriteAllLines(LogicPath,LogicInfo.ToArray());
					MessageBox.Show("保存成功!");
					LoadLogic();
				}
			}
			else
			{
				if(LogicCostMoneyTextbox.Text != null && LogicCostMoneyTextbox.Text != "" && LogicTotalMoneyTextbox.Text != null && LogicTotalMoneyTextbox.Text != "" && LogicTicketNoTextbox.Text != null && LogicTicketNoTextbox.Text != "")
				{
					LogicInfo[0] = LogicCostMoneyTextbox.Text;
					LogicInfo[1] = LogicTotalMoneyTextbox.Text;
					LogicInfo[2] = LogicTicketNoTextbox.Text;
					File.WriteAllLines(LogicPath,LogicInfo.ToArray());
					MessageBox.Show("保存成功!");
					LoadLogic();
				}
				else
				{
					MessageBox.Show("请填写正确的参数!");
				}
			}
		}
	}
}
