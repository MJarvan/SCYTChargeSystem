using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		/// 今日营业额统计
		/// </summary>
		List<decimal> TodayTotalMoney = new List<decimal>();

		/// <summary>
		/// 逻辑路径
		/// </summary>
		private static string LogicPath;

		/// <summary>
		/// 主页datatable
		/// </summary>
		DataTable maindt = new DataTable();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			LoadLogic();
			LoadMain();
		}

		#region 公式管理

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

		#endregion

		/// <summary>
		/// 获取主界面
		/// </summary>
		private void LoadMain()
		{
			MainAddPhoneNumberTextbox.Text = string.Empty;
			MainAddMoneyTextbox.Text = string.Empty;
			MainReceiveTicketTextblock.Text = string.Empty;

			//获取主页datagrid
			DbHelper db = new DbHelper();
			DbCommand selectmain = db.GetSqlStringCommond("select top(10)* from Ticket where State = 1 order by CreateDate ASC,UID ASC");
			maindt = db.ExecuteDataTable(selectmain);
			DataRow[] IsSelectedRows = maindt.Select("IsSelected=0");
			foreach(DataRow row in IsSelectedRows)
			{
				row["IsSelected"] = false;
			}
			DataRow[] stateRows = maindt.Select("State=1");
			foreach(DataRow row in stateRows)
			{
				row["State"] = "未领取";
			}
			MainTicketDatagrid.ItemsSource = maindt.DefaultView;

			//获取兑换券编号
			DbCommand TicketNomain = db.GetSqlStringCommond("select top(1)No from Ticket order by UID DESC");
			try
			{
				string lastestNo = db.ExecuteScalar(TicketNomain).ToString();
				string lastestNoFour = lastestNo.Remove(0,LogicTicketNoTextbox.Text.Length);
				if(lastestNo == "9999")
				{
					MessageBox.Show("请及时更换表头,否则将添加兑换券数据会出错");
				}
				else
				{
					MainAddTicketNoTextblock.Text = LogicTicketNoTextbox.Text + lastestNoFour;
					TicketNoIncrease(MainAddTicketNoTextblock.Text.Remove(0,LogicTicketNoTextbox.Text.Length));
				}
			}
			catch
			{
				MainAddTicketNoTextblock.Text = LogicTicketNoTextbox.Text + "0000";
			}


			//获取营业额
			//换了Money的表了请及时纠正
			LoadMoney();
			


			//获取其他信息
			DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
			int historynum = (int)db.ExecuteScalar(selecthistory);
			int nownum = (int)(Convert.ToDecimal(TotalMoneyTextblock.Text) / Convert.ToInt32(LogicTotalMoneyTextbox.Text)) + historynum;
			TotalSendNumTextblock.Text = nownum.ToString();
			if(historynum != nownum)
			{
				using(Trans t = new Trans())
				{
					try
					{
						DoBussiness.UpdateTicketNum(t,nownum);
						t.Commit();
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message + " ,添加历史兑换券数量失败,请联系管理员");
						t.RollBack();
					}
				}
			}


			DbCommand SendTicketNummain = db.GetSqlStringCommond("select * from Ticket where UseDate between dateadd(hh,-18,Datename(year,GetDate())+'-'+Datename(month,GetDate())+'-'+Datename(day,GetDate()))and DATEADD(day,1,dateadd(hh,-18,Datename(year,GetDate()) + '-' + Datename(month,GetDate()) + '-' + Datename(day,GetDate()))) and state = 2");
			DataTable SendTicketNum = db.ExecuteDataTable(SendTicketNummain);
			SendNumTextblock.Text = SendTicketNum.Rows.Count.ToString();
			RemainNumTextblock.Text = (Convert.ToInt32(TotalSendNumTextblock.Text) - Convert.ToInt32(SendNumTextblock.Text)).ToString();
		}

		private void LoadMoney()
		{
			DbHelper db = new DbHelper();
			TimeSpan ts = DateTime.Now - Convert.ToDateTime(DateTime.Today.ToShortDateString() + " 06:00:00");
			DbCommand GetTodayTotalMoneymain;
			DataTable TodayTotalMoneydt;
			decimal TodayTotalMoney;
			if(ts.TotalHours > 0)
			{
				GetTodayTotalMoneymain = db.GetSqlStringCommond("SELECT TotalMoney FROM Money where StartTime = dateadd(hh,+6,Datename(year,GetDate())+'-'+Datename(month,GetDate())+'-'+Datename(day,GetDate())) and EndTime = DATEADD(day,1,dateadd(hh,+6,Datename(year,GetDate()) + '-' + Datename(month,GetDate()) + '-' + Datename(day,GetDate())))");
				DataTable moneydt = db.ExecuteDataTable(GetTodayTotalMoneymain);

				//没有今天的记录,要创建
				if(moneydt.Rows.Count == 0)
				{
					using(Trans t = new Trans())
					{
						try
						{
							DoBussiness.AddMoney(t,0);
							t.Commit();
							LoadMoney();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + " ,添加营业额统计失败,请联系管理员");
							t.RollBack();
						}
					}
				}
				else
				{
					TodayTotalMoneydt = db.ExecuteDataTable(GetTodayTotalMoneymain);
					TodayTotalMoney = 0;
					for(int i = 0;i < TodayTotalMoneydt.Rows.Count;i++)
					{
						for(int j = 0;j < TodayTotalMoneydt.Columns.Count;j++)
						{
							TodayTotalMoney = TodayTotalMoney + (decimal)TodayTotalMoneydt.Rows[i][j];
						}
					}
					TotalMoneyTextblock.Text = Math.Round(TodayTotalMoney,2).ToString();
				}
			}
			else
			{
				GetTodayTotalMoneymain = db.GetSqlStringCommond("SELECT TotalMoney FROM Money where StartTime = dateadd(hh,-18,Datename(year,GetDate())+'-'+Datename(month,GetDate())+'-'+Datename(day,GetDate())) and EndTime = DATEADD(day,1,dateadd(hh,-18,Datename(year,GetDate()) + '-' + Datename(month,GetDate()) + '-' + Datename(day,GetDate())))");
			}
			TodayTotalMoneydt = db.ExecuteDataTable(GetTodayTotalMoneymain);
			TodayTotalMoney = 0;
			for(int i = 0;i < TodayTotalMoneydt.Rows.Count;i++)
			{
				for(int j = 0;j < TodayTotalMoneydt.Columns.Count;j++)
				{
					TodayTotalMoney = TodayTotalMoney + (decimal)TodayTotalMoneydt.Rows[i][j];
				}
			}
			TotalMoneyTextblock.Text = Math.Round(TodayTotalMoney,2).ToString();
		}

		/// <summary>
		/// 主页添加按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainAddTicketButton_Click(object sender,RoutedEventArgs e)
		{
			if(MainAddPhoneNumberTextbox.Text != null && MainAddPhoneNumberTextbox.Text != "" && MainAddMoneyTextbox.Text != null && MainAddMoneyTextbox.Text != "")
			{
				int AddNum = Convert.ToInt32(MainReceiveTicketTextblock.Text);
				if(AddNum >= 1)
				{
					string copyTicketNo = MainAddTicketNoTextblock.Text;
					using(Trans t = new Trans())
					{
						try
						{
							string phone = MainAddPhoneNumberTextbox.Text;
							decimal money = Convert.ToDecimal(MainAddMoneyTextbox.Text);
							for(int i = 0;i < AddNum;i++)
							{
								string no = MainAddTicketNoTextblock.Text;
								DoBussiness.AddTicket(t,no,phone,money);
								TicketNoIncrease(MainAddTicketNoTextblock.Text.Remove(0,LogicTicketNoTextbox.Text.Length));
							}
							DoBussiness.UpdateMoney(t,money);
							t.Commit();
							MessageBox.Show("添加成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + ",添加失败,请联系管理员");
							t.RollBack();
							MainAddTicketNoTextblock.Text = copyTicketNo;
						}
					}
				}
				else
				{
					MessageBox.Show("消费额度未满,不能发放兑换券,但营业额仍会更新");
					using(Trans t = new Trans())
					{
						try
						{
							decimal money = Convert.ToDecimal(MainAddMoneyTextbox.Text);
							DoBussiness.UpdateMoney(t,money);
							t.Commit();
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message);
							t.RollBack();
						}
					}
				}
			}
			else
			{
				MessageBox.Show("请填写完整信息");
			}
		}

		/// <summary>
		/// 兑换券编号递增
		/// </summary>
		/// <param name="noEnd"></param>
		private void TicketNoIncrease(string noEnd)
		{
			int no = Convert.ToInt32(noEnd);
			string four = string.Empty, three = string.Empty, two = string.Empty, one = string.Empty;

			if(no == 0)
			{
				string sum = "0001";
				MainAddTicketNoTextblock.Text = LogicTicketNoTextbox.Text + sum;
			}
			else
			{
				no++;
				if(no / 1000 >= 1)
				{
					four = (no / 1000).ToString();
					no = no % 1000;
				}
				else
				{
					four = "0";
				}
				if(no / 100 >= 1)
				{
					three = (no / 100).ToString();
					no = no % 100;
				}
				else
				{
					three = "0";
				}
				if(no / 10 >= 1)
				{
					two = (no / 10).ToString();
					no = no % 10;
				}
				else
				{
					two = "0";
				}
				one = no.ToString();

				string sum = four + three + two + one;
				MainAddTicketNoTextblock.Text = LogicTicketNoTextbox.Text + sum;
			}
		}

		/// <summary>
		/// 消费金额输入检测
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainAddMoneyTextbox_TextChanged(object sender,TextChangedEventArgs e)
		{
			TextBox textbox = sender as TextBox;
			string text = textbox.Text.Trim();
			if(text != null && text != "")
			{
				Regex r = new Regex(@"^([0-9\.]*)$");
				if(r.IsMatch(textbox.Text.Trim()) == false)
				{
					textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1,1);
					textbox.SelectionStart = textbox.Text.Length;
				}
				else
				{
					decimal money = Convert.ToDecimal(textbox.Text);
					int logicmonty = Convert.ToInt32(LogicCostMoneyTextbox.Text);
					if(money / logicmonty >= 1)
					{
						MainReceiveTicketTextblock.Text = ((int)(money / logicmonty)).ToString();
					}
					else
					{
						MainReceiveTicketTextblock.Text = "0";
					}
				}
			}
		}

		/// <summary>
		/// 电话号码输入检测
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainAddPhoneNumberTextbox_TextChanged(object sender,TextChangedEventArgs e)
		{
			TextBox textbox = sender as TextBox;
			string text = textbox.Text.Trim();
			if(text != null && text != "")
			{
				Regex r = new Regex("^[0-9]*$");
				if(r.IsMatch(textbox.Text.Trim()) == false)
				{
					textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1,1);
					textbox.SelectionStart = textbox.Text.Length;
				}
			}
		}

		/// <summary>
		/// 主页领取按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainConfirmTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataRow[] drs = maindt.Select("IsSelected=True");
			if(drs.Length == 0)
			{
				MessageBox.Show("请选择要操作的行");
			}
			else
			{
				if(Convert.ToInt32(TotalSendNumTextblock.Text) > 0)
				{
					if(MessageBox.Show("是否确认兑换这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
						bool IsSucceed = true;
						using(Trans t = new Trans())
						{
							try
							{
								foreach(DataRow dr in drs)
								{
									string state = "2";
									int UID = Convert.ToInt32(dr["UID"].ToString());
									DoBussiness.MainTicketStateChange(t,state,UID);
								}
								t.Commit();
							}
							catch(Exception ex)
							{
								IsSucceed = false;
								MessageBox.Show(ex.Message);
								t.RollBack();
							}
							finally
							{
								if(IsSucceed)
								{
									MessageBox.Show("领取成功");
									LoadMain();
								}
								else
								{
									MessageBox.Show("领取失败");
								}
							}
						}
					}
				}
				else
				{
					MessageBox.Show("今天没有可用发放兑换券的额度");
				}
			}
		}

		private void MainOverTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataRow[] drs = maindt.Select("IsSelected=True");
			if(drs.Length == 0)
			{
				MessageBox.Show("请选择要操作的行");
			}
			else
			{
				if(MessageBox.Show("是否确认过号这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
				{
					bool IsSucceed = true;
					using(Trans t = new Trans())
					{
						try
						{
							foreach(DataRow dr in drs)
							{
								string state = "0";
								int UID = Convert.ToInt32(dr["UID"].ToString());
								DoBussiness.MainTicketStateChange(t,state,UID);
							}
							t.Commit();
						}
						catch(Exception ex)
						{
							IsSucceed = false;
							MessageBox.Show(ex.Message);
							t.RollBack();
						}
						finally
						{
							if(IsSucceed)
							{
								MessageBox.Show("过号成功");
								LoadMain();
							}
							else
							{
								MessageBox.Show("过号失败");
							}
						}
					}
				}
			}
		}
	}
}
