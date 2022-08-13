using Common;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace tool.Controllers
{
    [ApiController]
    [Route("api/shiyuan")]
    public class ShiyuanController : ControllerBase
    {
        /// <summary>
        /// 获取期数
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLoanDeadline")]
        public IActionResult GetLoanDeadline()
        {
            List<Deadline> deadlines = new List<Deadline>();
            for (int i = 1; i <= Enum.GetValues(typeof(EnumDeadline)).Length; i++)
            {
                deadlines.Add(new Deadline { Id=i,Name=Algorithm.GetEnumDeadlineString((EnumDeadline)i) });
            }
            return new JsonResult(new { Success=true,Message=deadlines});
        }

        /// <summary>
        /// 获取期数
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRepayWay")]
        public IActionResult GetRepayWay()
        {
            List<Deadline> deadlines = new List<Deadline>();
            for (int i = 0; i < Enum.GetValues(typeof(EnumRepaymentWay)).Length; i++)
            {
                deadlines.Add(new Deadline { Id = i, Name = ((EnumRepaymentWay)i).ToString() });
            }
            return new JsonResult(new { Success = true, Message = deadlines });
        }

        /// <summary>
        /// 计算收益
        /// </summary>
        /// <param name="deadline">期限</param>
        /// <param name="payway">还款方式</param>
        /// <param name="rate">利率</param>
        /// <param name="money">借款金额</param>
        /// <param name="offerRate">额外利息</param>
        /// <param name="rateType">利率类型 0：年利率 1：月利率</param>
        /// <returns></returns>
        [HttpGet("Calculate")]
        public IActionResult Calculate(int deadline,int payway,decimal rate,decimal money,int offerRate,int rateType)
        {
            int deadlineInvest = Algorithm.GetDeadlinePeriod(Algorithm.GetEnumDeadline(deadline));
            EnumRepaymentWay repayWay = Algorithm.GetEnumRepaymentWay(payway);
            if (rateType==1)
            {
                rate = rate * 12;
            }
            Algorithm ag = new Algorithm(repayWay, deadlineInvest, rate, money, offerRate, 0);
            DataTable AlgorithmTable = ag.AlgorithmTable;

            EnumDeadline EnDeadline = Algorithm.GetEnumDeadline(deadline);
            decimal totalInterest = 0;
            decimal PriceInterest = 0;
            PriceInterest = (decimal)AlgorithmTable.Rows[0]["Interest"] + (decimal)AlgorithmTable.Rows[0]["PrincipalPrice"];
            if (repayWay == EnumRepaymentWay.到期本息)
                totalInterest = (decimal)AlgorithmTable.Rows[AlgorithmTable.Rows.Count - 1]["Interest"];
            else
                totalInterest = (decimal)AlgorithmTable.Rows[AlgorithmTable.Rows.Count - 1]["TotalInterest"];
            Statistic statistic = new Statistic() { AllInterest=totalInterest.ToString("n2"), Payments=new List<Payment>() };
            #region 列表
            if (repayWay == EnumRepaymentWay.到期本息)
            {
                deadlineInvest = 1;
                #region 循环
                statistic.Payments.Add(new Payment
                {
                    Row = 1,
                    RepayAll = (decimal.Parse(AlgorithmTable.Rows[deadlineInvest - 1]["Interest"].ToString()) + decimal.Parse(AlgorithmTable.Rows[deadlineInvest - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                    Principal = (decimal.Parse(AlgorithmTable.Rows[deadlineInvest - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                    Interest = (decimal.Parse(AlgorithmTable.Rows[deadlineInvest - 1]["Interest"].ToString())).ToString("n2"),
                    RemainPrincipal = "0.00"
                }) ;
                #endregion
            }
            else
            {
                if (deadline > 36)
                {
                    #region 循环
                    //循环生成期数
                    for (int i = 1; i <= deadlineInvest; i++)
                    {
                        decimal maintain = 0m;
                        for (int j = 0; j < i; j++)
                        {
                            maintain += decimal.Parse(AlgorithmTable.Rows[j]["PrincipalPrice"].ToString());
                        }
                        statistic.Payments.Add(new Payment
                        {
                            Row = i,
                            RepayAll = (decimal.Parse(AlgorithmTable.Rows[i - 1]["Interest"].ToString()) + decimal.Parse(AlgorithmTable.Rows[i - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                            Principal = (decimal.Parse(AlgorithmTable.Rows[i - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                            Interest = (decimal.Parse(AlgorithmTable.Rows[i - 1]["Interest"].ToString())).ToString("n2"),
                            RemainPrincipal = (money - maintain).ToString("n2")
                        }) ;
                    }
                    #endregion
                }
                else
                {
                    #region 循环
                    for (int i = 1; i <= deadlineInvest; i++)
                    {
                       decimal maintain = 0m;
                        for (int j = 0; j < i; j++)
                        {
                            maintain += decimal.Parse(AlgorithmTable.Rows[j]["PrincipalPrice"].ToString());
                        }
                        statistic.Payments.Add(new Payment
                        {
                            Row = i,
                            RepayAll = (decimal.Parse(AlgorithmTable.Rows[i - 1]["Interest"].ToString()) + decimal.Parse(AlgorithmTable.Rows[i - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                            Principal = (decimal.Parse(AlgorithmTable.Rows[i - 1]["PrincipalPrice"].ToString())).ToString("n2"),
                            Interest = (decimal.Parse(AlgorithmTable.Rows[i - 1]["Interest"].ToString())).ToString("n2"),
                            RemainPrincipal = (money - maintain).ToString("n2")
                        });
                    }
                    #endregion
                }
            }
            #endregion
            return new JsonResult(new { Success = true, Message = statistic });
        }
    }
    class Deadline
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Statistic
    {
        public string AllInterest { get; set; }
        public List<Payment> Payments { get; set; }
    }

    class Payment
    {
        /// <summary>
        /// 期数
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 还款总额
        /// </summary>
        public string RepayAll { get; set; }

        /// <summary>
        /// 本金
        /// </summary>
        public string Principal { get; set; }

        /// <summary>
        /// 利息
        /// </summary>
        public string Interest { get; set; }

        /// <summary>
        /// 剩余本金
        /// </summary>
        public string RemainPrincipal { get; set; }
    }
}