# 时薪计算器

三端同一套公式与进位规则，结果逐条比对一致：

| 端 | 文件 | 说明 |
|---|---|---|
| Windows | `时薪计算器.exe` | 单文件，双击即用，不用装运行时 |
| 网页 | `index.html` | 单文件，有浏览器就能用（Windows / Linux / macOS / 手机） |
| Linux / macOS | `hourlywage.sh` | 终端版，只依赖 `sh` + `awk` |

## 计算口径

```
时薪 =（月薪 + 奖金）÷（每日工时 × 21.75 + 节假日总工时）

月计薪天数 =（365 − 104）÷ 12 = 21.75 天/月
```

21.75 是年均值：一年 365 天，扣掉 52 周 × 2 天周末共 104 天，剩 261 天计薪，平均到每月 21.75 天。

结果四舍五入到分，半分向上进位（三端同一规则）。输入留空按 0 计算；总工时算出来是 0、或填了负数时直接显示原因。

## 用法

### Windows

双击 `时薪计算器.exe`。填 4 个数，中间的大数字就是时薪，没有按钮，边填边算。

### 网页

浏览器打开 `index.html` 即可，界面与 Windows 版一致。

想在线访问：仓库 Settings → Pages → Source 选 `main` 分支根目录，之后打开 `https://llllishuo.github.io/HourlyWage/`。

### Linux / macOS（终端）

```sh
chmod +x hourlywage.sh          # 第一次
./hourlywage.sh 8000 8 0 0      # 月薪 每日工时 节假日总工时 奖金 → 45.98
./hourlywage.sh                 # 不带参数则交互输入，直接回车取默认值
./hourlywage.sh -h              # 帮助
```

只输出数字，方便接管道：

```sh
rate=$(./hourlywage.sh 8000 8 0 0); echo "时薪 $rate 元/小时"
```

参数非法时打印原因并以退出码 2 结束。

## 三端一致性测试

```sh
sh test/consistency.sh
```

同一组输入分别交给 awk / JS / C# 计算并逐条比对，包含半分进位（45.125 → 45.13）这类边界。JS 需要 `node`；C# 需要先编译出 `wage-test.exe`，没有会自动跳过。

## 从源码构建

### Windows

```bat
build.cmd
```

或手动：

```bat
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /codepage:65001 /optimize+ ^
  /out:"时薪计算器.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll ^
  src\Wage.cs src\Program.cs
```

`/codepage:65001` 不能省，否则 UTF-8 源码里的中文会被按本地代码页误读。

自检程序（一致性测试要用）：

```bat
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /codepage:65001 ^
  /out:wage-test.exe src\Wage.cs src\TestMain.cs
wage-test.exe
```

### 网页 / 终端版

无需构建，改完即用。

## 目录

| 文件 | 作用 |
|---|---|
| `src/Wage.cs` | 计算内核：公式、月计薪天数、校验、进位 |
| `src/Program.cs` | Windows 界面（WinForms） |
| `src/TestMain.cs` | 控制台自检，并输出一致性测试用的 `VALUE` 行 |
| `index.html` | 网页版（跨平台，单文件） |
| `hourlywage.sh` | 终端版（Linux / macOS，零依赖） |
| `test/consistency.sh` | 三端一致性检查 |
| `时薪计算器.exe` | Windows 编译产物 |

## 改口径时注意

月计薪天数写在三个地方，改要一起改：

1. `src/Wage.cs` — `PaidDaysPerYear` / `PayDaysPerMonth`
2. `index.html` — `wage-logic` 段里的 `PAID_DAYS_PER_YEAR`
3. `hourlywage.sh` — `PAY_DAYS_PER_MONTH`

改完跑一遍 `sh test/consistency.sh`，确认三端仍然一致。
