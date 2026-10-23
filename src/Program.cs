using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HourlyWage
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    /// <summary>四个输入框，边填边算，结果就是屏幕上那个数。</summary>
    internal sealed class MainForm : Form
    {
        private static readonly Font UiFont = new Font("Microsoft YaHei UI", 10.5F);
        private static readonly Font BigFont = new Font("Microsoft YaHei UI", 30F, FontStyle.Bold);
        private static readonly Font SmallFont = new Font("Microsoft YaHei UI", 8.5F);
        private static readonly Font TipFont = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        private static readonly Color Muted = Color.FromArgb(138, 142, 148);
        private static readonly Color Warn = Color.FromArgb(196, 60, 60);
        private static readonly Color Result = Color.FromArgb(24, 90, 160);

        private readonly TextBox _salary;
        private readonly TextBox _hoursPerDay;
        private readonly TextBox _holidayHours;
        private readonly TextBox _bonus;
        private readonly TextBox[] _inputs;
        private readonly Label _value;

        public MainForm()
        {
            SuspendLayout();

            Text = "时薪计算器";
            Font = UiFont;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(400, 296);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;

            _salary = AddInput("月薪（元）", "8000", 18);
            _hoursPerDay = AddInput("每日工时（小时）", "8", 54);
            _holidayHours = AddInput("节假日总工时（小时）", "0", 90);
            _bonus = AddInput("奖金（元）", "0", 126);
            _inputs = new TextBox[] { _salary, _hoursPerDay, _holidayHours, _bonus };

            Panel line = new Panel();
            line.BackColor = Color.FromArgb(233, 235, 238);
            line.Location = new Point(22, 164);
            line.Size = new Size(356, 1);
            Controls.Add(line);

            Label caption = new Label();
            caption.Text = "时薪（元/小时）";
            caption.Location = new Point(22, 170);
            caption.Size = new Size(356, 16);
            caption.TextAlign = ContentAlignment.MiddleCenter;
            caption.Font = SmallFont;
            caption.ForeColor = Muted;
            Controls.Add(caption);

            _value = new Label();
            _value.Location = new Point(22, 186);
            _value.Size = new Size(356, 58);
            _value.TextAlign = ContentAlignment.MiddleCenter;
            _value.Font = BigFont;
            _value.ForeColor = Result;
            Controls.Add(_value);

            Label note = new Label();
            note.Text = "月计薪天数按年均 21.75 天；时薪 =（月薪 + 奖金）÷ 总工时";
            note.Location = new Point(22, 250);
            note.Size = new Size(356, 34);
            note.TextAlign = ContentAlignment.TopCenter;
            note.Font = SmallFont;
            note.ForeColor = Muted;
            Controls.Add(note);

            foreach (TextBox box in _inputs)
            {
                box.TextChanged += delegate { UpdateResult(); };
            }

            ResumeLayout(false);

            UpdateResult();
            _salary.Focus();
            _salary.SelectAll();
        }

        private TextBox AddInput(string label, string value, int y)
        {
            Label lb = new Label();
            lb.Text = label;
            lb.Location = new Point(22, y);
            lb.Size = new Size(136, 26);
            lb.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(lb);

            TextBox box = new TextBox();
            box.Text = value;
            box.Location = new Point(166, y);
            box.Size = new Size(168, 26);
            box.TextAlign = HorizontalAlignment.Right;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = UiFont;
            Controls.Add(box);

            return box;
        }

        /// <summary>边填边算：任一输入框变化就重算一次。</summary>
        private void UpdateResult()
        {
            if (_value == null)
            {
                return;
            }

            double salary, hoursPerDay, holidayHours, bonus;

            if (!TryRead(_salary, out salary) || !TryRead(_hoursPerDay, out hoursPerDay)
                || !TryRead(_holidayHours, out holidayHours) || !TryRead(_bonus, out bonus))
            {
                ShowTip("请填数字");
                return;
            }

            double hourly;
            string error;

            if (!WageCalculator.TryCompute(salary, hoursPerDay, holidayHours, bonus, out hourly, out error))
            {
                ShowTip(error);
                return;
            }

            _value.Font = BigFont;
            _value.ForeColor = Result;
            _value.Text = hourly.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void ShowTip(string message)
        {
            _value.Font = TipFont;
            _value.ForeColor = Warn;
            _value.Text = message;
        }

        private static bool TryRead(TextBox box, out double value)
        {
            string text = (box.Text ?? string.Empty).Trim();
            text = text.Replace('，', '.').Replace('．', '.').Replace('。', '.').Replace(",", string.Empty);
            text = text.Replace(" ", string.Empty);

            if (text.Length == 0)
            {
                value = 0;
                return true;
            }

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
