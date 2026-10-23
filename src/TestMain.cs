using System;
using System.Globalization;
using HourlyWage;

/// <summary>控制台自检：与界面共用同一份计算内核。</summary>
internal static class TestMain
{
    private static int _failed;

    private static void Main()
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch (Exception)
        {
        }

        double hourly;
        string error;

        // 月计薪天数必须来自年均值：（365 − 104）÷ 12 = 21.75
        ExpectNear(21.75, WageCalculator.PayDaysPerMonth, "月计薪天数 =（365 − 104）÷ 12 = 21.75");

        // 月薪 8000 + 奖金 1000，每日 8 小时，节假日 10 小时 → 9000 ÷ (8×21.75 + 10) = 9000/184
        Expect(WageCalculator.TryCompute(8000, 8, 10, 1000, out hourly, out error), "正常计算");
        ExpectNear(Round2(9000.0 / 184.0), hourly, "含奖金与节假日工时的时薪");
        Expect(Format(hourly) == "48.91", "结果两位小数 → 48.91");

        // 无奖金、无节假日：8000 ÷ 174 = 45.977 → 45.98
        WageCalculator.TryCompute(8000, 8, 0, 0, out hourly, out error);
        ExpectNear(Round2(8000.0 / 174.0), hourly, "纯月薪的时薪");
        Expect(Format(hourly) == "45.98", "结果两位小数 → 45.98");

        // 进位边界：7851.75 ÷ 174 = 45.125 正好落在半分上，必须向上进位（三端同一规则）
        WageCalculator.TryCompute(7851.75, 8, 0, 0, out hourly, out error);
        Expect(Format(hourly) == "45.13", "半分进位 45.125 → 45.13");

        // 异常输入必须被拒绝
        Expect(!WageCalculator.TryCompute(0, 0, 0, 0, out hourly, out error) && !string.IsNullOrEmpty(error),
            "总工时为 0 被拒绝");
        Expect(!WageCalculator.TryCompute(-1, 8, 0, 0, out hourly, out error) && !string.IsNullOrEmpty(error),
            "负月薪被拒绝");
        Expect(!WageCalculator.TryCompute(8000, 8, -1, 0, out hourly, out error) && !string.IsNullOrEmpty(error),
            "负工时被拒绝");

        // 供 test/consistency.sh 与终端版、网页版逐条比对
        Console.WriteLine();
        PrintValue(8000, 8, 0, 0);
        PrintValue(8000, 8, 10, 1000);
        PrintValue(12000, 7.5, 20, 2000);
        PrintValue(6000, 8, 0, 0);
        PrintValue(5000, 0, 100, 0);
        PrintValue(0, 8, 0, 0);
        PrintValue(100000, 8, 0, 50000);
        PrintValue(7851.75, 8, 0, 0);

        Console.WriteLine(_failed == 0 ? "计算自检全部通过" : "失败项：" + _failed.ToString(CultureInfo.InvariantCulture));
    }

    private static void PrintValue(double salary, double hours, double holiday, double bonus)
    {
        double hourly;
        string error;

        string text = WageCalculator.TryCompute(salary, hours, holiday, bonus, out hourly, out error)
            ? Format(hourly)
            : "ERR";

        Console.WriteLine("VALUE " + Num(salary) + " " + Num(hours) + " " + Num(holiday) + " " + Num(bonus) + " " + text);
    }

    private static double Round2(double value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string Format(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string Num(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static void Expect(bool condition, string name)
    {
        if (condition)
        {
            Console.WriteLine("[PASS] " + name);
            return;
        }

        _failed++;
        Console.WriteLine("[FAIL] " + name);
    }

    private static void ExpectNear(double expected, double actual, string name)
    {
        if (Math.Abs(expected - actual) <= 1e-9)
        {
            Console.WriteLine("[PASS] " + name);
            return;
        }

        _failed++;
        Console.WriteLine("[FAIL] " + name + " → 期望 " + expected.ToString("R", CultureInfo.InvariantCulture)
            + "，实际 " + actual.ToString("R", CultureInfo.InvariantCulture));
    }
}
