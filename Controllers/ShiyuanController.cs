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
        /// ��ȡ����
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
        /// ��ȡ����
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
        /// ��������
        /// </summary>
        /// <param name="deadline">����</param>
        /// <param name="payway">���ʽ</param>
        /// <param name="rate">����</param>
        /// <param name="money">�����</param>
        /// <param name="offerRate">������Ϣ</param>
        /// <param name="rateType">�������� 0�������� 1��������</param>
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
            if (repayWay == EnumRepaymentWay.���ڱ�Ϣ)
                totalInterest = (decimal)AlgorithmTable.Rows[AlgorithmTable.Rows.Count - 1]["Interest"];
            else
                totalInterest = (decimal)AlgorithmTable.Rows[AlgorithmTable.Rows.Count - 1]["TotalInterest"];
            Statistic statistic = new Statistic() { AllInterest=totalInterest.ToString("n2"), Payments=new List<Payment>() };
            #region �б�
            if (repayWay == EnumRepaymentWay.���ڱ�Ϣ)
            {
                deadlineInvest = 1;
                #region ѭ��
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
                    #region ѭ��
                    //ѭ����������
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
                    #region ѭ��
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
        /// ����
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// �����ܶ�
        /// </summary>
        public string RepayAll { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Principal { get; set; }

        /// <summary>
        /// ��Ϣ
        /// </summary>
        public string Interest { get; set; }

        /// <summary>
        /// ʣ�౾��
        /// </summary>
        public string RemainPrincipal { get; set; }
    }
}