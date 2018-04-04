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
		#region 属性变量
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

		/// <summary>
		/// 管理页datatable
		/// </summary>
		DataTable managedt = new DataTable();

		/// <summary>
		/// 统计页datatable
		/// </summary>
		DataTable moneydt = new DataTable();

		#endregion 属性变量

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			LoadLogic();
			LoadMain();
			AddManagePage();
			InitManageComboBox();
			AddMoneyPage();
			InitMoneyComboBox();
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

				if(LogicInfo.Count >= 2)
				{
					LogicCostMoneyTextbox.Text = LogicInfo[0];
					LogicTotalMoneyTextbox.Text = LogicInfo[1];
					LogicTotalMoneyReadTextbox.Text = LogicTotalMoneyTextbox.Text;
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
				LogicInfo.Add("100");
				LogicInfo.Add("2000");
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
				if(LogicCostMoneyTextbox.Text != null && LogicCostMoneyTextbox.Text != "" && LogicTotalMoneyTextbox.Text != null && LogicTotalMoneyTextbox.Text != "")
				{
					LogicInfo[0] = LogicCostMoneyTextbox.Text;
					LogicInfo[1] = LogicTotalMoneyTextbox.Text;
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

		#region 主页

		/// <summary>
		/// 获取主界面
		/// </summary>
		private void LoadMain()
		{
			MainAddPhoneNumberTextbox.Text = string.Empty;
			MainAddMoneyTextbox.Text = string.Empty;
			MainAddTicketNoTextbox.Text = string.Empty;
			MainReceiveTicketTextbox.Text = string.Empty;

			ManageAddPhoneNumberTextbox.Text = string.Empty;
			ManageAddMoneyTextbox.Text = string.Empty;
			ManageReceiveTicketTextbox.Text = string.Empty;
			ManageAddTicketNoTextbox.Text = string.Empty;
			ManageDeleteMoneyTextbox.Text = string.Empty;
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

			//获取管理页datagrid
			DbCommand selectmanage = db.GetSqlStringCommond("select top(10)* from Ticket where State != 3 order by CreateDate DESC,UID DESC");
			managedt = db.ExecuteDataTable(selectmanage);
			DataRow[] IsSelectedRows1 = managedt.Select("IsSelected=0");
			foreach(DataRow row in IsSelectedRows1)
			{
				row["IsSelected"] = false;
				switch(Convert.ToInt32(row["State"]))
				{
					case 0:
						{
							row["State"] = "过号";
							break;
						}
					case 1:
						{
							row["State"] = "未领取";
							break;
						}
					case 2:
						{
							row["State"] = "已领取";
							break;
						}
				}
			}
			ManageTicketDatagrid.ItemsSource = managedt.DefaultView;

			//获取营业额
			LoadMoney();

			//获取其他信息
			DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
			int historynum = (int)db.ExecuteScalar(selecthistory);
			DbCommand SendTicketNummain = db.GetSqlStringCommond("select * from Ticket where UseDate between dateadd(hh,+6,Datename(year,GetDate())+'-'+Datename(month,GetDate())+'-'+Datename(day,GetDate()))and DATEADD(day,1,dateadd(hh,+6,Datename(year,GetDate()) + '-' + Datename(month,GetDate()) + '-' + Datename(day,GetDate()))) and state = 2");
			DataTable SendTicketNum = db.ExecuteDataTable(SendTicketNummain);
			SendNumTextblock.Text = SendTicketNum.Rows.Count.ToString();
			DataTable dt = DoBussiness.TotalSendNumQuery(DateTime.Now.ToShortDateString());
			TotalSendNumTextblock.Text = dt.Rows.Count.ToString();
			RemainNumTextblock.Text = historynum.ToString();
		}


		/// <summary>
		/// 获取营业额
		/// </summary>
		private void LoadMoney()
		{
			DbHelper db = new DbHelper();
			TimeSpan ts = DateTime.Now - Convert.ToDateTime(DateTime.Today.ToShortDateString() + " 06:00:00");
			DbCommand GetTodayTotalMoneymain;
			DataTable TodayTotalMoneydt;
			decimal TodayTotalMoney;
			//过了六点,今天
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
			//没过六点,昨天
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
		/// 主页金额添加按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainAddMoneyButton_Click(object sender,RoutedEventArgs e)
		{
			if(MainAddMoneyTextbox.Text != null && MainAddMoneyTextbox.Text != "")
			{
				decimal oldmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
				decimal logicmoney = Convert.ToDecimal(LogicTotalMoneyTextbox.Text);
				int oldticketnum = (int)(oldmoney / logicmoney);
				using(Trans t = new Trans())
				{
					try
					{
						decimal money = Convert.ToDecimal(MainAddMoneyTextbox.Text);
						DoBussiness.UpdateMoneyAdd(t,money);
						t.Commit();
						LoadMoney();
						decimal newmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
						int newticketnum = (int)(newmoney / logicmoney);
						if(oldticketnum != newticketnum)
						{
							DbHelper db = new DbHelper();
							DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
							int historynum = (int)db.ExecuteScalar(selecthistory);
							DoBussiness.UpdateTicketNum(null,historynum + (newticketnum - oldticketnum));
						}
						MessageBox.Show("添加成功");
						LoadMain();
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message + ",添加失败,请联系管理员");
						t.RollBack();
					}
				}
			}
			else
			{
				MessageBox.Show("请填写完整信息");
			}
		}
	
		/// <summary>
		/// 主页enter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainTextbox_KeyDown(object sender,KeyEventArgs e)
		{
			TextBox textbox = sender as TextBox;
			if(e.Key == Key.Enter)
			{
				switch(textbox.Tag.ToString())
				{
					case "0":
						{
							MainAddMoneyButton_Click(sender,e);
							break;
						}
					case "1":
						{
							MainReceiveTicketTextbox.Focus();
							break;
						}
					case "2":
						{
							MainAddPhoneNumberTextbox.Focus();
							break;
						}
					case "3":
						{
							MainAddTicketButton_Click(sender,e);
							break;
						}
				}
			}
		}

		/// <summary>
		/// 主页兑换券添加按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainAddTicketButton_Click(object sender,RoutedEventArgs e)
		{
			if(MainAddTicketNoTextbox.Text != null && MainAddTicketNoTextbox.Text != "" && MainReceiveTicketTextbox.Text != null && MainReceiveTicketTextbox.Text != "" && MainAddPhoneNumberTextbox.Text != null && MainAddPhoneNumberTextbox.Text != "" && MainAddTicketNoTextbox.Text.Length > 4)
			{
				int AddNum = Convert.ToInt32(MainReceiveTicketTextbox.Text);
				if(AddNum >= 1)
				{
					using(Trans t = new Trans())
					{
						try
						{
							string phone = MainAddPhoneNumberTextbox.Text;
							for(int i = 0;i < AddNum;i++)
							{
								string no = MainAddTicketNoTextbox.Text;
								DoBussiness.AddTicket(t,no,phone);
								DoBussiness.UpdateSendNumAdd(t);
								MainAddTicketNoTextbox.Text = TicketNoIncrease(no);
							}
							t.Commit();
							MessageBox.Show("添加成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + ",添加失败,请联系管理员");
							t.RollBack();
						}
					}
				}
				else
				{
					MessageBox.Show("领券数量必须大于等于1");
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
		/// <param name="OldTicketNo"></param>
		private string TicketNoIncrease(string OldTicketNo)
		{
			string NewTicketNo = string.Empty;
			string tickettop = OldTicketNo.Remove(OldTicketNo.Length - 4,4);
			int no = Convert.ToInt32(OldTicketNo.Remove(0,OldTicketNo.Length - 4));
			string four = string.Empty, three = string.Empty, two = string.Empty, one = string.Empty;

			if(no == 0)
			{
				NewTicketNo = tickettop + "0001";
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

				NewTicketNo = tickettop + four + three + two + one;
			}
			return NewTicketNo;
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
				DbHelper db = new DbHelper();
				DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
				int historynum = (int)db.ExecuteScalar(selecthistory);
				if(historynum > 0)
				{
					if(drs.Length <= historynum)
					{
						if(MessageBox.Show("是否确认兑换这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
						{
							using(Trans t = new Trans())
							{
								try
								{
									foreach(DataRow dr in drs)
									{
										string state = "2";
										int UID = Convert.ToInt32(dr["UID"].ToString());
										DoBussiness.MainTicketStateChange(t,state,UID);
										DoBussiness.UpdateExchangeNum(t);
										DoBussiness.UpdateTicketNum(t,historynum - 1);
									}
									t.Commit();
									MessageBox.Show("领取成功");
									LoadMain();
								}
								catch(Exception ex)
								{
									MessageBox.Show(ex.Message + " ,领取失败,请联系管理员");
									t.RollBack();
								}
							}
						}
					}
					else
					{
						MessageBox.Show("已经超过发放兑换券的额度,请重新选择");
					}
				}
				else
				{
					MessageBox.Show("今天没有可用发放兑换券的额度");
				}
			}
		}

		/// <summary>
		/// 主页过号按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
							MessageBox.Show("过号成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + " ,过号失败,请联系管理员");
							t.RollBack();
						}
					}
				}
			}
		}

		#endregion 主页

		#region 兑换券管理
		/// <summary>
		/// 管理页营业额添加按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageAddTicketButton_Click(object sender,RoutedEventArgs e)
		{
			if(ManageAddTicketNoTextbox.Text != null && ManageAddTicketNoTextbox.Text != "" && ManageReceiveTicketTextbox.Text != null && ManageReceiveTicketTextbox.Text != "" && ManageAddPhoneNumberTextbox.Text != null && ManageAddPhoneNumberTextbox.Text != "" && ManageAddTicketNoTextbox.Text.Length > 4)
			{
				int AddNum = Convert.ToInt32(ManageReceiveTicketTextbox.Text);
				if(AddNum >= 1)
				{
					using(Trans t = new Trans())
					{
						try
						{
							string phone = ManageAddPhoneNumberTextbox.Text;
							for(int i = 0;i < AddNum;i++)
							{
								string no = ManageAddTicketNoTextbox.Text;
								DoBussiness.AddTicket(t,no,phone);
								DoBussiness.UpdateSendNumAdd(t);
								ManageAddTicketNoTextbox.Text = TicketNoIncrease(no);
							}
							t.Commit();
							MessageBox.Show("添加成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + ",添加失败,请联系管理员");
							t.RollBack();
						}
					}
				}
				else
				{
					MessageBox.Show("领券数量必须大于等于1");
				}
			}
			else
			{
				MessageBox.Show("请填写完整信息");
			}
		}

		/// <summary>
		/// 管理页营业额添加或删除按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageMoneyButton_Click(object sender,RoutedEventArgs e)
		{
			Button button = sender as Button;
			bool AddOrDelete = Convert.ToBoolean(button.Tag);
			//添加营业额
			if(AddOrDelete)
			{
				if(ManageAddMoneyTextbox.Text != null && ManageAddMoneyTextbox.Text != "")
				{
					decimal oldmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
					decimal logicmoney = Convert.ToDecimal(LogicTotalMoneyTextbox.Text);
					int oldticketnum = (int)(oldmoney / logicmoney);
					using(Trans t = new Trans())
					{
						try
						{
							decimal money = Convert.ToDecimal(ManageAddMoneyTextbox.Text);
							DoBussiness.UpdateMoneyAdd(t,money);
							t.Commit();
							LoadMoney();
							decimal newmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
							int newticketnum = (int)(newmoney / logicmoney);
							if(oldticketnum != newticketnum)
							{
								DbHelper db = new DbHelper();
								DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
								int historynum = (int)db.ExecuteScalar(selecthistory);
								DoBussiness.UpdateTicketNum(null,historynum + (newticketnum - oldticketnum));
							}
							MessageBox.Show("添加成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + ",添加失败,请联系管理员");
							t.RollBack();
						}
					}
				}
				else
				{
					MessageBox.Show("请填写完整信息");
				}
			}
			//删除营业额
			else
			{
				if(ManageDeleteMoneyTextbox.Text != null && ManageDeleteMoneyTextbox.Text != "")
				{
					decimal oldmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
					decimal logicmoney = Convert.ToDecimal(LogicTotalMoneyTextbox.Text);
					int oldticketnum = (int)(oldmoney / logicmoney);
					using(Trans t = new Trans())
					{
						try
						{
							decimal money = Convert.ToDecimal(ManageDeleteMoneyTextbox.Text);
							DoBussiness.UpdateMoneyDelete(t,money);
							t.Commit();
							LoadMoney();
							decimal newmoney = Convert.ToDecimal(TotalMoneyTextblock.Text);
							int newticketnum = (int)(newmoney / logicmoney);
							if(oldticketnum != newticketnum)
							{
								DbHelper db = new DbHelper();
								DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
								int historynum = (int)db.ExecuteScalar(selecthistory);
								DoBussiness.UpdateTicketNum(null,historynum - (oldticketnum - newticketnum));
							}
							MessageBox.Show("删除成功");
							LoadMain();
						}
						catch(Exception ex)
						{
							MessageBox.Show(ex.Message + ",删除失败,请联系管理员");
							t.RollBack();
						}
					}
				}
				else
				{
					MessageBox.Show("请填写完整信息");
				}
			}
		}

		/// <summary>
		/// 管理页enter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageTextbox_KeyDown(object sender,KeyEventArgs e)
		{
			TextBox textbox = sender as TextBox;
			if(e.Key == Key.Enter)
			{
				switch(textbox.Tag.ToString())
				{
					case "4":
						{
							ManageMoneyButton_Click(ManageAddMoneyButton,e);
							break;
						}
					case "5":
						{
							ManageMoneyButton_Click(ManageDeleteMoneyButton,e);
							break;
						}
					case "6":
						{
							ManageReceiveTicketTextbox.Focus();
							break;
						}
					case "7":
						{
							ManageAddPhoneNumberTextbox.Focus();
							break;
						}
					case "8":
						{
							ManageAddTicketButton_Click(sender,e);
							break;
						}
				}
			}
		}

		/// <summary>
		/// 管理页领取按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageConfirmTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataTable dt = (ManageTicketDatagrid.ItemsSource as DataView).ToTable();
			if(dt.Rows.Count > 0)
			{
				DataRow[] drs = dt.Select("IsSelected=True");
				if(drs.Length == 0)
				{
					MessageBox.Show("请选择要操作的行");
				}
				else
				{
					foreach(DataRow dr in drs)
					{
						if(dr["State"].ToString() == "已领取")
						{
							MessageBox.Show("已领取的券不允许状态更改");
							return;
						}
					}
					DbHelper db = new DbHelper();
					DbCommand selecthistory = db.GetSqlStringCommond("select * from TicketNum");
					int historynum = (int)db.ExecuteScalar(selecthistory);
					if(historynum > 0)
					{
						if(drs.Length <= historynum)
						{
							if(MessageBox.Show("是否确认兑换这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
							{
								using(Trans t = new Trans())
								{
									try
									{
										foreach(DataRow dr in drs)
										{
											string state = "2";
											int UID = Convert.ToInt32(dr["UID"].ToString());
											DoBussiness.MainTicketStateChange(t,state,UID);
											DoBussiness.UpdateExchangeNum(t);
											DoBussiness.UpdateTicketNum(t,historynum - 1);
										}
										t.Commit();
										MessageBox.Show("领取成功");
										LoadMain();
									}
									catch(Exception ex)
									{
										MessageBox.Show(ex.Message + " ,领取失败,请联系管理员");
										t.RollBack();
									}
								}
							}
						}
						else
						{
							MessageBox.Show("已经超过发放兑换券的额度,请重新选择");
						}
					}
					else
					{
						MessageBox.Show("今天没有可用发放兑换券的额度");
					}
				}
			}
			else
			{
				MessageBox.Show("请先查询数据");
			}
		}

		/// <summary>
		/// 管理页过号按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageOverTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataTable dt = (ManageTicketDatagrid.ItemsSource as DataView).ToTable();
			if(dt.Rows.Count > 0)
			{
				DataRow[] drs = dt.Select("IsSelected=True");
				if(drs.Length == 0)
				{
					MessageBox.Show("请选择要操作的行");
				}
				else
				{
					foreach(DataRow dr in drs)
					{
						if(dr["State"].ToString() == "已领取")
						{
							MessageBox.Show("已领取的券不允许状态更改");
							return;
						}
					}
					if(MessageBox.Show("是否确认过号这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
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
								MessageBox.Show("过号成功");
								LoadMain();
							}
							catch(Exception ex)
							{
								MessageBox.Show(ex.Message + " ,过号失败,请联系管理员");
								t.RollBack();
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("请先查询数据");
			}
		}

		/// <summary>
		/// 管理页重新排队
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageRequeueTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataTable dt = (ManageTicketDatagrid.ItemsSource as DataView).ToTable();
			if(dt.Rows.Count > 0)
			{
				DataRow[] drs = dt.Select("IsSelected=True");
				if(drs.Length == 0)
				{
					MessageBox.Show("请选择要操作的行");
				}
				else
				{
					foreach(DataRow dr in drs)
					{
						if(dr["State"].ToString() == "已领取")
						{
							MessageBox.Show("已领取的券不允许状态更改");
							return;
						}
					}
					if(MessageBox.Show("是否确认重新排队这些券?","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
						using(Trans t = new Trans())
						{
							try
							{
								foreach(DataRow dr in drs)
								{
									string state = "1";
									int UID = Convert.ToInt32(dr["UID"].ToString());
									DoBussiness.MainTicketStateChange(t,state,UID);
								}
								t.Commit();
								MessageBox.Show("重新排队成功");
								LoadMain();
							}
							catch(Exception ex)
							{
								MessageBox.Show(ex.Message + " ,重新排队失败,请联系管理员");
								t.RollBack();
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("请先查询数据");
			}
		}

		/// <summary>
		/// 兑换券删除
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ManageDeleteTicketButton_Click(object sender,RoutedEventArgs e)
		{
			DataTable dt = (ManageTicketDatagrid.ItemsSource as DataView).ToTable();
			if(dt.Rows.Count > 0)
			{
				DataRow[] drs = dt.Select("IsSelected=True");
				if(drs.Length == 0)
				{
					MessageBox.Show("请选择要操作的行");
				}
				else
				{
					foreach(DataRow dr in drs)
					{
						if(dr["State"].ToString() == "已领取")
						{
							MessageBox.Show("已领取的券不允许状态更改");
							return;
						}
						TimeSpan ts = DateTime.Now - Convert.ToDateTime(DateTime.Today.ToShortDateString() + " 06:00:00");
						//今天
						if(ts.TotalHours > 0)
						{
							if(Convert.ToDateTime(dr["CreateDate"]).ToShortDateString() != DateTime.Today.ToShortDateString())
							{
								MessageBox.Show("只能删除当天的兑换券,请重新操作");
								return;
							}
						}
						//昨天
						else
						{
							if(Convert.ToDateTime(dr["CreateDate"]).ToShortDateString() != DateTime.Now.AddDays(-1).ToShortDateString())
							{
								MessageBox.Show("只能删除当天的兑换券,请重新操作");
								return;
							}
						}
					}
					if(MessageBox.Show("是否确认删除这些券?(删除后不可恢复)","提醒",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
						using(Trans t = new Trans())
						{
							try
							{

								foreach(DataRow dr in drs)
								{
									string state = "3";
									int UID = Convert.ToInt32(dr["UID"].ToString());
									DoBussiness.MainTicketStateChange(t,state,UID);
								}
								DoBussiness.UpdateSendNumDelete(t);
								t.Commit();
								MessageBox.Show("删除成功");
								LoadMain();
							}
							catch(Exception ex)
							{
								MessageBox.Show(ex.Message + " ,删除失败,请联系管理员");
								t.RollBack();
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("请先查询数据");
			}
		}
		/// <summary>
		/// 兑换券查询
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void QueryTicketButton_Click(object sender,RoutedEventArgs e)
		{
			string state = string.Empty;
			string createdate = string.Empty;
			string querytext = string.Empty;
			if(QueryTicketCombobox.SelectedItem != null)
			{
				switch(QueryTicketCombobox.Text.ToString())
				{
					case "过号":
						{
							state = "0";
							break;
						}
					case "未领取":
						{
							state = "1";
							break;
						}
					case "已领取":
						{
							state = "2";
							break;
						}
					//case "删除":
					//	{
					//		state = "3";
					//		break;
					//	}
					default:
						{
							state = string.Empty;
							break;
						}
				}
			}
			if(QueryTicketDatepicker.Text != null || QueryTicketDatepicker.Text != "")
			{
				createdate = QueryTicketDatepicker.Text.Trim();
			}
			if(QueryTicketNoTextbox.Text != null || QueryTicketNoTextbox.Text != "")
			{
				querytext = "%" + QueryTicketNoTextbox.Text.Trim() + "%";
			}

			managedt = DoBussiness.ManageQuery(state,createdate,querytext);
			if(managedt != null)
			{
				DataRow[] IsSelectedRows = managedt.Select("IsSelected=0");
				foreach(DataRow row in IsSelectedRows)
				{
					row["IsSelected"] = false;
					switch(Convert.ToInt32(row["State"]))
					{
						case 0:
							{
								row["State"] = "过号";
								break;
							}
						case 1:
							{
								row["State"] = "未领取";
								break;
							}
						case 2:
							{
								row["State"] = "已领取";
								break;
							}
					}
				}
				ManageTicketDatagrid.ItemsSource = managedt.DefaultView;
				ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
				ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,1);
			}
		}
		#endregion 兑换券管理

		#region 营业额管理
		/// <summary>
		/// 营业额查询
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void QueryMoneyButton_Click(object sender,RoutedEventArgs e)
		{
			string startime = string.Empty;
			string endtime = string.Empty;
			if(QueryStartDatepicker.Text != null && QueryStartDatepicker.Text != "")
			{
				startime = QueryStartDatepicker.Text.Trim();
			}
			else
			{
				MessageBox.Show("请选择开始日期");
				return;
			}
			if(QueryEndDatepicker.Text != null && QueryEndDatepicker.Text != "")
			{
				endtime = QueryEndDatepicker.Text.Trim();
			}
			else
			{
				MessageBox.Show("请选择结束日期");
				return;
			}
			moneydt = DoBussiness.TotalQuery(startime,endtime);

			if(moneydt != null)
			{
				QueryDatagrid.ItemsSource = moneydt.DefaultView;
				ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
				MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,1);
			}
		}

		#endregion 营业额管理

		#region 兑换券分页

		#region 添加分页控件

		/// <summary>
		/// 添加分页控件
		/// </summary>
		private void AddManagePage()
		{

			ComboBox combobox = new ComboBox();
			combobox.Name = "comboBoxmanagePageNumber";
			combobox.Width = double.NaN;
			combobox.VerticalAlignment = VerticalAlignment.Center;
			combobox.SelectionChanged += new SelectionChangedEventHandler(comboBoxmanagePageNumber_SelectionChanged);
			managestackpanel.Children.Add(combobox);

			Button button1 = new Button();
			button1.Name = "buttonmanageHome";
			button1.Content = "首页";
			button1.VerticalAlignment = VerticalAlignment.Center;
			button1.Margin = new Thickness(3,0,3,0);
			button1.Click += new RoutedEventHandler(buttonmanageHome_Click);
			managestackpanel.Children.Add(button1);

			Button button2 = new Button();
			button2.Name = "buttonmanageUp";
			button2.Content = "上一页";
			button2.VerticalAlignment = VerticalAlignment.Center;
			button2.Margin = new Thickness(3,0,3,0);
			button2.Click += new RoutedEventHandler(buttonmanageUp_Click);
			managestackpanel.Children.Add(button2);

			Button button3 = new Button();
			button3.Name = "buttonmanageNext";
			button3.Content = "下一页";
			button3.VerticalAlignment = VerticalAlignment.Center;
			button3.Margin = new Thickness(3,0,3,0);
			button3.Click += new RoutedEventHandler(buttonmanageNext_Click);
			managestackpanel.Children.Add(button3);

			Button button4 = new Button();
			button4.Name = "buttonmanageEnd";
			button4.Content = "尾页";
			button4.VerticalAlignment = VerticalAlignment.Center;
			button4.Margin = new Thickness(3,0,3,0);
			button4.Click += new RoutedEventHandler(buttonmanageEnd_Click);
			managestackpanel.Children.Add(button4);

			TextBlock textblock1 = new TextBlock();
			textblock1.Text = "共";
			textblock1.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock1);

			TextBlock textblock2 = new TextBlock();
			textblock2.Text = "0";
			textblock2.Name = "textBlockmanageTotal";
			textblock2.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock2);

			TextBlock textblock3 = new TextBlock();
			textblock3.Text = "条";
			textblock3.VerticalAlignment = VerticalAlignment.Center;
			textblock3.Margin = new Thickness(3,0,3,0);
			managestackpanel.Children.Add(textblock3);

			TextBlock textblock4 = new TextBlock();
			textblock4.Text = "第";
			textblock4.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock4);

			TextBlock textblock5 = new TextBlock();
			textblock5.Text = "0";
			textblock5.VerticalAlignment = VerticalAlignment.Center;
			textblock5.Name = "textBlockPage";
			managestackpanel.Children.Add(textblock5);

			TextBlock textblock6 = new TextBlock();
			textblock6.Text = "页";
			textblock6.VerticalAlignment = VerticalAlignment.Center;
			textblock6.Margin = new Thickness(3,0,3,0);
			managestackpanel.Children.Add(textblock6);

			TextBlock textblock7 = new TextBlock();
			textblock7.Text = "/";
			textblock7.VerticalAlignment = VerticalAlignment.Center;
			textblock7.Margin = new Thickness(3,0,3,0);
			managestackpanel.Children.Add(textblock7);

			TextBlock textblock8 = new TextBlock();
			textblock8.Text = "共";
			textblock8.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock8);

			TextBlock textblock9 = new TextBlock();
			textblock9.Text = "0";
			textblock9.Name = "textBlockmanageTotalPage";
			textblock9.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock9);

			TextBlock textblock30 = new TextBlock();
			textblock30.Text = "页";
			textblock30.VerticalAlignment = VerticalAlignment.Center;
			textblock30.Margin = new Thickness(3,0,3,0);
			managestackpanel.Children.Add(textblock30);

			TextBlock textblock10 = new TextBlock();
			textblock10.Text = "转到";
			textblock10.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock10);

			TextBox textbox = new TextBox();
			textbox.Name = "textBoxmanagePageNumber";
			textbox.TextAlignment = TextAlignment.Center;
			textbox.VerticalAlignment = VerticalAlignment.Center;
			textbox.Width = 70;
			textbox.TextChanged += new TextChangedEventHandler(textBoxmanagePageNumber_TextChanged);
			managestackpanel.Children.Add(textbox);

			TextBlock textblock20 = new TextBlock();
			textblock20.Text = "页";
			textblock20.VerticalAlignment = VerticalAlignment.Center;
			managestackpanel.Children.Add(textblock20);

			Button button5 = new Button();
			button5.Name = "buttonmanageOK";
			button5.Content = "GO";
			button5.VerticalAlignment = VerticalAlignment.Center;
			button5.Margin = new Thickness(3,0,3,0);
			button5.Click += new RoutedEventHandler(buttonmanageOK_Click);
			managestackpanel.Children.Add(button5);
		}

		#endregion 添加分页控件

		/// <summary>
		/// 主页按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmanageHome_Click(object sender,RoutedEventArgs e)
		{
			if(managedt != null)
			{
				//if (currentPage < totalPage)
				{
					ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");

					ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,1);
				}
			}
		}

		/// <summary>
		/// 向上按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmanageUp_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockPage");
			int currentPage = int.Parse(textBlockPage.Text);
			if(managedt != null)
			{
				if(currentPage > 1)
				{
					ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
					ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,currentPage - 1);
				}
			}
		}

		/// <summary>
		/// 下一个按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmanageNext_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockPage");
			int currentPage = int.Parse(textBlockPage.Text);
			TextBlock textBlockmanageTotalPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockmanageTotalPage");
			int totalPage = int.Parse(textBlockmanageTotalPage.Text);
			if(managedt != null)
			{
				if(currentPage < totalPage)
				{
					ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
					ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,currentPage + 1);
				}
			}
		}

		/// <summary>
		///  结束按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmanageEnd_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockmanageTotalPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockmanageTotalPage");
			int totalPage = int.Parse(textBlockmanageTotalPage.Text);
			if(managedt != null)
			{
				//if (currentPage < totalPage)
				{
					ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
					ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,totalPage);
				}
			}
		}

		/// <summary>
		/// 完成按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmanageOK_Click(object sender,RoutedEventArgs e)
		{
			if(managedt != null)
			{
				ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
				TextBox textBoxmanagePageNumber = GetChildObject<TextBox>(this.managestackpanel,"textBoxmanagePageNumber");
				ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,int.Parse(textBoxmanagePageNumber.Text));
			}
		}

		/// <summary>
		/// 显示页面条数
		/// </summary>
		/// <param name="pageNumber">当前页显示的条数</param>
		/// <param name="currentPage">当前页</param>
		private DataTable ManagePage(DataTable dt,int pageNumber,int currentPage)
		{
			DataTable dataTablePage = new DataTable();
			dataTablePage = dt.Clone();
			int total = dt.Rows.Count;

			int totalPage = 0;//总页数
			if(total % pageNumber == 0)
			{
				totalPage = total / pageNumber;
			}
			else
			{
				totalPage = total / pageNumber + 1;
			}

			int first = pageNumber * (currentPage - 1);//当前记录是多少条
			first = (first > 0) ? first : 0;
			//如果总数量大于每页显示数量  
			if(total >= pageNumber * currentPage)
			{
				for(int i = first;i < pageNumber * currentPage;i++)
					dataTablePage.ImportRow(dt.Rows[i]);
			}
			else
			{
				for(int i = first;i < dt.Rows.Count;i++)
					dataTablePage.ImportRow(dt.Rows[i]);
			}

			this.ManageTicketDatagrid.ItemsSource = dataTablePage.DefaultView;
			//	tmpTable.Dispose();
			TextBlock textBlockmanageTotal = GetChildObject<TextBlock>(this.managestackpanel,"textBlockmanageTotal");
			TextBlock textBlockmanageTotalPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockmanageTotalPage");
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockPage");

			textBlockmanageTotal.Text = total.ToString();
			textBlockmanageTotalPage.Text = totalPage.ToString();
			textBlockPage.Text = currentPage.ToString();

			ManageButonStatus(currentPage,totalPage);
			return dataTablePage;
		}

		/// <summary>
		/// 按钮状态
		/// </summary>
		/// <param name="currentPage">当前页</param>
		/// <param name="totalPage">总页数</param>
		private void ManageButonStatus(int currentPage,int totalPage)
		{
			Button buttonmanageHome = GetChildObject<Button>(this.managestackpanel,"buttonmanageHome");
			Button buttonmanageUp = GetChildObject<Button>(this.managestackpanel,"buttonmanageUp");
			Button buttonmanageEnd = GetChildObject<Button>(this.managestackpanel,"buttonmanageEnd");
			Button buttonmanageNext = GetChildObject<Button>(this.managestackpanel,"buttonmanageNext");

			if(currentPage == 1)
			{
				buttonmanageHome.IsEnabled = false;
				buttonmanageUp.IsEnabled = false;
			}
			else
			{
				buttonmanageHome.IsEnabled = true;
				buttonmanageUp.IsEnabled = true;
			}

			if(currentPage == totalPage)
			{
				buttonmanageEnd.IsEnabled = false;
				buttonmanageNext.IsEnabled = false;
			}
			else
			{
				buttonmanageEnd.IsEnabled = true;
				buttonmanageNext.IsEnabled = true;
			}
		}

		/// <summary>
		/// 组合框页码_选择已更改事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBoxmanagePageNumber_SelectionChanged(object sender,SelectionChangedEventArgs e)
		{
			if(managedt != null)
			{
				ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");
				TextBox textBoxmanagePageNumber = GetChildObject<TextBox>(this.managestackpanel,"textBoxmanagePageNumber");
				ManagePage(managedt,(int)comboBoxmanagePageNumber.SelectedValue,1);
				textBoxmanagePageNumber.Text = "";
			}
		}

		/// <summary>
		/// 初始化组合框
		/// </summary>
		private void InitManageComboBox()
		{
			ComboBox comboBoxmanagePageNumber = GetChildObject<ComboBox>(this.managestackpanel,"comboBoxmanagePageNumber");

			Dictionary<int,string> dicComboBox = new Dictionary<int,string>()
			{
				{5,"每页显示5条"},
				{10,"每页显示10条"},
				{20,"每页显示20条"},
				{50,"每页显示50条"}
			};

			comboBoxmanagePageNumber.ItemsSource = null;
			comboBoxmanagePageNumber.SelectedValuePath = "Key";
			comboBoxmanagePageNumber.DisplayMemberPath = "Value";
			comboBoxmanagePageNumber.ItemsSource = dicComboBox;

			comboBoxmanagePageNumber.SelectedIndex = 1;
		}

		/// <summary>
		/// 文本框页码_文本已更改事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textBoxmanagePageNumber_TextChanged(object sender,TextChangedEventArgs e)
		{
			string strNumber = string.Empty;
			TextBox textBoxmanagePageNumber = GetChildObject<TextBox>(this.managestackpanel,"textBoxmanagePageNumber");
			TextBlock textBlockmanageTotalPage = GetChildObject<TextBlock>(this.managestackpanel,"textBlockmanageTotalPage");

			foreach(char charText in textBoxmanagePageNumber.Text.Trim())
			{
				int intOut = 0;
				if(Int32.TryParse(charText.ToString(),out intOut))
				{
					strNumber = strNumber + charText.ToString();
				}
			}

			if(strNumber != string.Empty)
			{
				if(Convert.ToDecimal(strNumber) > Convert.ToDecimal(textBlockmanageTotalPage.Text))
				{
					strNumber = textBlockmanageTotalPage.Text;
				}
				if(Convert.ToDecimal(strNumber) < 1)
				{
					strNumber = "1";
				}
			}
			else
			{
				strNumber = "1";
			}

			textBoxmanagePageNumber.Text = strNumber;
		}

		#endregion 分页

		#region 营业额分页

		#region 添加分页控件

		/// <summary>
		/// 添加分页控件
		/// </summary>
		private void AddMoneyPage()
		{

			ComboBox combobox = new ComboBox();
			combobox.Name = "comboBoxmoneyPageNumber";
			combobox.Width = double.NaN;
			combobox.VerticalAlignment = VerticalAlignment.Center;
			combobox.SelectionChanged += new SelectionChangedEventHandler(comboBoxmoneyPageNumber_SelectionChanged);
			moneystackpanel.Children.Add(combobox);

			Button button1 = new Button();
			button1.Name = "buttonmoneyHome";
			button1.Content = "首页";
			button1.VerticalAlignment = VerticalAlignment.Center;
			button1.Margin = new Thickness(3,0,3,0);
			button1.Click += new RoutedEventHandler(buttonmoneyHome_Click);
			moneystackpanel.Children.Add(button1);

			Button button2 = new Button();
			button2.Name = "buttonmoneyUp";
			button2.Content = "上一页";
			button2.VerticalAlignment = VerticalAlignment.Center;
			button2.Margin = new Thickness(3,0,3,0);
			button2.Click += new RoutedEventHandler(buttonmoneyUp_Click);
			moneystackpanel.Children.Add(button2);

			Button button3 = new Button();
			button3.Name = "buttonmoneyNext";
			button3.Content = "下一页";
			button3.VerticalAlignment = VerticalAlignment.Center;
			button3.Margin = new Thickness(3,0,3,0);
			button3.Click += new RoutedEventHandler(buttonmoneyNext_Click);
			moneystackpanel.Children.Add(button3);

			Button button4 = new Button();
			button4.Name = "buttonmoneyEnd";
			button4.Content = "尾页";
			button4.VerticalAlignment = VerticalAlignment.Center;
			button4.Margin = new Thickness(3,0,3,0);
			button4.Click += new RoutedEventHandler(buttonmoneyEnd_Click);
			moneystackpanel.Children.Add(button4);

			TextBlock textblock1 = new TextBlock();
			textblock1.Text = "共";
			textblock1.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock1);

			TextBlock textblock2 = new TextBlock();
			textblock2.Text = "0";
			textblock2.Name = "textBlockmoneyTotal";
			textblock2.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock2);

			TextBlock textblock3 = new TextBlock();
			textblock3.Text = "条";
			textblock3.VerticalAlignment = VerticalAlignment.Center;
			textblock3.Margin = new Thickness(3,0,3,0);
			moneystackpanel.Children.Add(textblock3);

			TextBlock textblock4 = new TextBlock();
			textblock4.Text = "第";
			textblock4.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock4);

			TextBlock textblock5 = new TextBlock();
			textblock5.Text = "0";
			textblock5.VerticalAlignment = VerticalAlignment.Center;
			textblock5.Name = "textBlockPage";
			moneystackpanel.Children.Add(textblock5);

			TextBlock textblock6 = new TextBlock();
			textblock6.Text = "页";
			textblock6.VerticalAlignment = VerticalAlignment.Center;
			textblock6.Margin = new Thickness(3,0,3,0);
			moneystackpanel.Children.Add(textblock6);

			TextBlock textblock7 = new TextBlock();
			textblock7.Text = "/";
			textblock7.VerticalAlignment = VerticalAlignment.Center;
			textblock7.Margin = new Thickness(3,0,3,0);
			moneystackpanel.Children.Add(textblock7);

			TextBlock textblock8 = new TextBlock();
			textblock8.Text = "共";
			textblock8.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock8);

			TextBlock textblock9 = new TextBlock();
			textblock9.Text = "0";
			textblock9.Name = "textBlockmoneyTotalPage";
			textblock9.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock9);

			TextBlock textblock30 = new TextBlock();
			textblock30.Text = "页";
			textblock30.VerticalAlignment = VerticalAlignment.Center;
			textblock30.Margin = new Thickness(3,0,3,0);
			moneystackpanel.Children.Add(textblock30);

			TextBlock textblock10 = new TextBlock();
			textblock10.Text = "转到";
			textblock10.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock10);

			TextBox textbox = new TextBox();
			textbox.Name = "textBoxmoneyPageNumber";
			textbox.TextAlignment = TextAlignment.Center;
			textbox.VerticalAlignment = VerticalAlignment.Center;
			textbox.Width = 70;
			textbox.TextChanged += new TextChangedEventHandler(textBoxmoneyPageNumber_TextChanged);
			moneystackpanel.Children.Add(textbox);

			TextBlock textblock20 = new TextBlock();
			textblock20.Text = "页";
			textblock20.VerticalAlignment = VerticalAlignment.Center;
			moneystackpanel.Children.Add(textblock20);

			Button button5 = new Button();
			button5.Name = "buttonmoneyOK";
			button5.Content = "GO";
			button5.VerticalAlignment = VerticalAlignment.Center;
			button5.Margin = new Thickness(3,0,3,0);
			button5.Click += new RoutedEventHandler(buttonmoneyOK_Click);
			moneystackpanel.Children.Add(button5);
		}

		#endregion 添加分页控件

		/// <summary>
		/// 主页按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmoneyHome_Click(object sender,RoutedEventArgs e)
		{
			if(moneydt != null)
			{
				//if (currentPage < totalPage)
				{
					ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");

					MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,1);
				}
			}
		}

		/// <summary>
		/// 向上按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmoneyUp_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockPage");
			int currentPage = int.Parse(textBlockPage.Text);
			if(moneydt != null)
			{
				if(currentPage > 1)
				{
					ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
					MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,currentPage - 1);
				}
			}
		}

		/// <summary>
		/// 下一个按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmoneyNext_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockPage");
			int currentPage = int.Parse(textBlockPage.Text);
			TextBlock textBlockmoneyTotalPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockmoneyTotalPage");
			int totalPage = int.Parse(textBlockmoneyTotalPage.Text);
			if(moneydt != null)
			{
				if(currentPage < totalPage)
				{
					ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
					MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,currentPage + 1);
				}
			}
		}

		/// <summary>
		///  结束按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmoneyEnd_Click(object sender,RoutedEventArgs e)
		{
			TextBlock textBlockmoneyTotalPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockmoneyTotalPage");
			int totalPage = int.Parse(textBlockmoneyTotalPage.Text);
			if(moneydt != null)
			{
				//if (currentPage < totalPage)
				{
					ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
					MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,totalPage);
				}
			}
		}

		/// <summary>
		/// 完成按钮_单击事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonmoneyOK_Click(object sender,RoutedEventArgs e)
		{
			if(moneydt != null)
			{
				ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
				TextBox textBoxmoneyPageNumber = GetChildObject<TextBox>(this.moneystackpanel,"textBoxmoneyPageNumber");
				MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,int.Parse(textBoxmoneyPageNumber.Text));
			}
		}

		/// <summary>
		/// 显示页面条数
		/// </summary>
		/// <param name="pageNumber">当前页显示的条数</param>
		/// <param name="currentPage">当前页</param>
		private DataTable MoneyPage(DataTable dt,int pageNumber,int currentPage)
		{
			DataTable dataTablePage = new DataTable();
			dataTablePage = dt.Clone();
			int total = dt.Rows.Count;

			int totalPage = 0;//总页数
			if(total % pageNumber == 0)
			{
				totalPage = total / pageNumber;
			}
			else
			{
				totalPage = total / pageNumber + 1;
			}

			int first = pageNumber * (currentPage - 1);//当前记录是多少条
			first = (first > 0) ? first : 0;
			//如果总数量大于每页显示数量  
			if(total >= pageNumber * currentPage)
			{
				for(int i = first;i < pageNumber * currentPage;i++)
					dataTablePage.ImportRow(dt.Rows[i]);
			}
			else
			{
				for(int i = first;i < dt.Rows.Count;i++)
					dataTablePage.ImportRow(dt.Rows[i]);
			}

			this.QueryDatagrid.ItemsSource = dataTablePage.DefaultView;
			//	tmpTable.Dispose();
			TextBlock textBlockmoneyTotal = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockmoneyTotal");
			TextBlock textBlockmoneyTotalPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockmoneyTotalPage");
			TextBlock textBlockPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockPage");

			textBlockmoneyTotal.Text = total.ToString();
			textBlockmoneyTotalPage.Text = totalPage.ToString();
			textBlockPage.Text = currentPage.ToString();

			MoneyButonStatus(currentPage,totalPage);
			return dataTablePage;
		}

		/// <summary>
		/// 按钮状态
		/// </summary>
		/// <param name="currentPage">当前页</param>
		/// <param name="totalPage">总页数</param>
		private void MoneyButonStatus(int currentPage,int totalPage)
		{
			Button buttonmoneyHome = GetChildObject<Button>(this.moneystackpanel,"buttonmoneyHome");
			Button buttonmoneyUp = GetChildObject<Button>(this.moneystackpanel,"buttonmoneyUp");
			Button buttonmoneyEnd = GetChildObject<Button>(this.moneystackpanel,"buttonmoneyEnd");
			Button buttonmoneyNext = GetChildObject<Button>(this.moneystackpanel,"buttonmoneyNext");

			if(currentPage == 1)
			{
				buttonmoneyHome.IsEnabled = false;
				buttonmoneyUp.IsEnabled = false;
			}
			else
			{
				buttonmoneyHome.IsEnabled = true;
				buttonmoneyUp.IsEnabled = true;
			}

			if(currentPage == totalPage)
			{
				buttonmoneyEnd.IsEnabled = false;
				buttonmoneyNext.IsEnabled = false;
			}
			else
			{
				buttonmoneyEnd.IsEnabled = true;
				buttonmoneyNext.IsEnabled = true;
			}
		}

		/// <summary>
		/// 组合框页码_选择已更改事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBoxmoneyPageNumber_SelectionChanged(object sender,SelectionChangedEventArgs e)
		{
			if(moneydt != null)
			{
				ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");
				TextBox textBoxmoneyPageNumber = GetChildObject<TextBox>(this.moneystackpanel,"textBoxmoneyPageNumber");
				MoneyPage(moneydt,(int)comboBoxmoneyPageNumber.SelectedValue,1);
				textBoxmoneyPageNumber.Text = "";
			}
		}

		/// <summary>
		/// 初始化组合框
		/// </summary>
		private void InitMoneyComboBox()
		{
			ComboBox comboBoxmoneyPageNumber = GetChildObject<ComboBox>(this.moneystackpanel,"comboBoxmoneyPageNumber");

			Dictionary<int,string> dicComboBox = new Dictionary<int,string>()
			{
				{5,"每页显示5条"},
				{10,"每页显示10条"},
				{20,"每页显示20条"},
				{50,"每页显示50条"}
			};

			comboBoxmoneyPageNumber.ItemsSource = null;
			comboBoxmoneyPageNumber.SelectedValuePath = "Key";
			comboBoxmoneyPageNumber.DisplayMemberPath = "Value";
			comboBoxmoneyPageNumber.ItemsSource = dicComboBox;

			comboBoxmoneyPageNumber.SelectedIndex = 1;
		}

		/// <summary>
		/// 文本框页码_文本已更改事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textBoxmoneyPageNumber_TextChanged(object sender,TextChangedEventArgs e)
		{
			string strNumber = string.Empty;
			TextBox textBoxmoneyPageNumber = GetChildObject<TextBox>(this.moneystackpanel,"textBoxmoneyPageNumber");
			TextBlock textBlockmoneyTotalPage = GetChildObject<TextBlock>(this.moneystackpanel,"textBlockmoneyTotalPage");

			foreach(char charText in textBoxmoneyPageNumber.Text.Trim())
			{
				int intOut = 0;
				if(Int32.TryParse(charText.ToString(),out intOut))
				{
					strNumber = strNumber + charText.ToString();
				}
			}

			if(strNumber != string.Empty)
			{
				if(Convert.ToDecimal(strNumber) > Convert.ToDecimal(textBlockmoneyTotalPage.Text))
				{
					strNumber = textBlockmoneyTotalPage.Text;
				}
				if(Convert.ToDecimal(strNumber) < 1)
				{
					strNumber = "1";
				}
			}
			else
			{
				strNumber = "1";
			}

			textBoxmoneyPageNumber.Text = strNumber;
		}

		#endregion 分页

		/// <summary>
		/// 根据控件名获取控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public T GetChildObject<T>(DependencyObject obj,string name) where T : FrameworkElement
		{
			DependencyObject child = null;
			T grandChild = null;

			for(int i = 0;i <= VisualTreeHelper.GetChildrenCount(obj) - 1;i++)
			{
				child = VisualTreeHelper.GetChild(obj,i);

				if(child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
				{
					return (T)child;
				}
				else
				{
					grandChild = GetChildObject<T>(child,name);
					if(grandChild != null)
						return grandChild;
				}
			}
			return null;
		}

	}
}
