using System;

namespace HourlyWage
{
    /// <summary>时薪 =（月薪 + 奖金）÷（每日工时 × 月计薪天数 + 节假日总工时）。</summary>
    public static class WageCalculator
    {
        /// <summary>一年 365 天，扣掉 52 周 × 2 天周末共 104 天，剩 261 天计薪。</summary>
        public const double PaidDaysPerYear = 365 - 104;

        /// <summary>月计薪天数按年平均：261 ÷ 12 = 21.75 天/月。</summary>
        public const double PayDaysPerMonth = PaidDaysPerYear / 12.0;

        /// <summary>计算时薪，失败时返回 false 并给出原因。</summary>
        public static bool TryCompute(double monthlySalary, double hoursPerDay, double holidayHours, double bonus,
                                      out double hourlyWage, out string error)
        {
            hourlyWage = 0;
            error = null;

            if (monthlySalary < 0 || hoursPerDay < 0 || holidayHours < 0 || bonus < 0)
            {
                error = "请输入不小于 0 的数字";
                return false;
            }

            double totalHours = hoursPerDay * PayDaysPerMonth + holidayHours;

            if (totalHours <= 0)
            {
                error = "总工时不能为 0";
                return false;
            }

            hourlyWage = (monthlySalary + bonus) / totalHours;
            return true;
        }
    }
}
