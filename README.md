# 时薪计算器

一个 7.7 KB 的单文件 Windows 小程序：填 4 个数字，直接看到自己的时薪。

## 用法

下载 `时薪计算器.exe`，双击运行。不需要装运行时——Win10 / Win11 自带 .NET Framework 4.x。

| 输入 | 说明 |
|---|---|
| 月薪（元） | |
| 每日工时（小时） | 默认 8 |
| 节假日总工时（小时） | 当月节假日/加班累计工时，没有就 0 |
| 奖金（元） | 当月奖金，没有就 0 |

边填边算，没有按钮。改任一格，中间的大数字立刻跟着变。

## 计算口径

```
时薪 =（月薪 + 奖金）÷（每日工时 × 21.75 + 节假日总工时）

月计薪天数 =（365 − 104）÷ 12 = 21.75 天/月
```

21.75 是年均值：一年 365 天，扣掉 52 周 × 2 天周末共 104 天，剩 261 天计薪，平均到每月 21.75 天。

结果保留两位小数。输入框留空按 0 计算；总工时算出来是 0、或填了负数时，会直接显示原因。

## 从源码构建

需要 Windows 自带的 C# 编译器（.NET Framework 4.x 附带）：

```bat
build.cmd
```

或手动执行：

```bat
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /codepage:65001 /optimize+ ^
  /out:"时薪计算器.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll ^
  src\Wage.cs src\Program.cs
```

`/codepage:65001` 不能省，否则 UTF-8 源码里的中文会被按本地代码页误读。

## 目录

| 文件 | 作用 |
|---|---|
| `src/Wage.cs` | 计算内核：公式、月计薪天数、输入校验 |
| `src/Program.cs` | WinForms 界面 |
| `src/TestMain.cs` | 控制台自检（与界面共用同一份内核） |
| `时薪计算器.exe` | 编译产物，可直接运行 |

改计算口径只需要动 `src/Wage.cs` 里的两个常量：

```csharp
public const double PaidDaysPerYear = 365 - 104;                    // 年计薪天数
public const double PayDaysPerMonth = PaidDaysPerYear / 12.0;       // → 21.75 天/月
```

要按「扣掉法定节假日」的口径，把 `365 - 104` 改成 `365 - 104 - 11`（法定节假日 11 天）即可。
