﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace SCYTChargeSystem
{
	public class DoBussiness
	{
		/// <summary>
		/// 生成兑换券
		/// </summary>
		/// <param name="t">事务</param>
		/// <param name="no">兑换券编号</param>
		/// <param name="phone">电话号码</param>
		/// <param name="money">金额</param>
		public static void AddTicket(Trans t,string no,string phone, decimal money)
		{
			DbHelper db = new DbHelper();
			DateTime TicketCreateTime = new DateTime();
			TicketCreateTime = DateTime.Now;
			DbCommand insert = db.GetSqlStringCommond("insert into Ticket values (@No, @Phone, @Money, @State, @CreateDate, @UseDate, @IsSelected)");

			db.AddInParameter(insert,"@No",DbType.String,no);
			db.AddInParameter(insert,"@Phone",DbType.String,phone);
			db.AddInParameter(insert,"@Money",DbType.Decimal,money);
			db.AddInParameter(insert,"@State",DbType.String,"1");
			db.AddInParameter(insert,"@CreateDate",DbType.DateTime,TicketCreateTime);
			db.AddInParameter(insert,"@UseDate",DbType.DateTime,DBNull.Value);
			db.AddInParameter(insert,"@IsSelected",DbType.Int32,0);

			if(t == null)
			{
				db.ExecuteNonQuery(insert);
			}
			else
			{
				db.ExecuteNonQuery(insert,t);
			}
		}

		public static void AddHistory(Trans t,int num,decimal money)
		{
			DbHelper db = new DbHelper();
			DateTime HistoryCreateDate = new DateTime();
			HistoryCreateDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
			DbCommand insert = db.GetSqlStringCommond("insert into History values (@HistoryTicketNum, @HistotyMoney, @CreateDate),StartTime = dateadd(hh,-18,Datename(year,GetDate())+'-'+Datename(month,GetDate())+'-'+Datename(day,GetDate())) , EndTime = DATEADD(day,1,dateadd(hh,-18,Datename(year,GetDate()) + '-' + Datename(month,GetDate()) + '-' + Datename(day,GetDate())))");

			db.AddInParameter(insert,"@HistoryTicketNum",DbType.Int32,num);
			db.AddInParameter(insert,"@HistotyMoney",DbType.Decimal,money);
			db.AddInParameter(insert,"@CreateDate",DbType.DateTime,HistoryCreateDate);

			if(t == null)
			{
				db.ExecuteNonQuery(insert);
			}
			else
			{
				db.ExecuteNonQuery(insert,t);
			}
		}

		public static void UpdateTicketNum(Trans t,int nownum)
		{
			DbHelper db = new DbHelper();
			DbCommand update = db.GetSqlStringCommond("update TicketNum set TicketNum= @TicketNum");

			db.AddInParameter(update,"@TicketNum",DbType.Int32,nownum);

			if(t == null)
			{
				db.ExecuteNonQuery(update);
			}
			else
			{
				db.ExecuteNonQuery(update,t);
			}
		}

		public static void MainTicketStateChange(Trans t,string state,int UID)
		{
			DbHelper db = new DbHelper();
			DateTime TicketUseTime = new DateTime();
			TicketUseTime = DateTime.Now;
			DbCommand update = db.GetSqlStringCommond("update Ticket set State= @State,UseDate= @UseDate where UID = @UID");

			db.AddInParameter(update,"@UID",DbType.Int32,UID);
			db.AddInParameter(update,"@State",DbType.String,state);
			db.AddInParameter(update,"@UseDate",DbType.DateTime,TicketUseTime);

			if(t == null)
			{
				db.ExecuteNonQuery(update);
			}
			else
			{
				db.ExecuteNonQuery(update,t);
			}
		}
	}
}
