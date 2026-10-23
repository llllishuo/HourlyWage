#!/bin/sh
# 三端一致性检查：终端版（awk）/ 网页版（JS）/ Windows 版（C#）
#
# 用法（在本目录执行即可）：sh test/consistency.sh
#   - 终端版：直接调用 ./hourlywage.sh
#   - 网页版：需要 node；从 index.html 里抽出 wage-logic 那段执行，不启动浏览器
#   - C# 版：需要先编译出 wage-test.exe（见 README），没有就跳过

cd "$(dirname "$0")/.." || exit 1

fail=0

compare() {
    if [ "$3" = "$2" ]; then
        printf '[PASS] %-22s → %s\n' "$1" "$3"
    else
        fail=$((fail + 1))
        printf '[FAIL] %-22s → 期望 %s，实际 %s\n' "$1" "$2" "$3"
    fi
}

cases='8000 8 0 0|45.98
8000 8 10 1000|48.91
12000 7.5 20 2000|76.45
6000 8 0 0|34.48
5000 0 100 0|50.00
0 8 0 0|0.00
100000 8 0 50000|862.07
7851.75 8 0 0|45.13'

echo "== 终端版 hourlywage.sh =="
while IFS='|' read -r args expected; do
    [ -z "$args" ] && continue
    got=$(sh ./hourlywage.sh $args 2>/dev/null)
    compare "sh $args" "$expected" "$got"
done <<CASES
$cases
CASES

echo "== 网页版 index.html =="
if command -v node >/dev/null 2>&1; then
    js_out=$(printf '%s\n' "$cases" | node -e '
const fs = require("fs");
const html = fs.readFileSync("index.html", "utf8");
const m = html.match(/<script id="wage-logic">([\s\S]*?)<\/script>/);
if (!m) { console.error("index.html 里找不到 wage-logic"); process.exit(3); }
const computeWage = new Function(m[1] + "\nreturn computeWage;")();
const lines = fs.readFileSync(0, "utf8").split("\n").filter(l => l.trim());
for (const line of lines) {
  const [args, expected] = line.split("|");
  const [s, h, o, b] = args.trim().split(/\s+/).map(Number);
  const r = computeWage(s, h, o, b);
  console.log(args.trim() + "|" + expected + "|" + (r.ok ? r.text : "ERR"));
}
')
    while IFS='|' read -r args expected got; do
        [ -z "$args" ] && continue
        compare "js $args" "$expected" "$got"
    done <<JS
$js_out
JS
else
    echo "  跳过：没有 node"
fi

echo "== Windows 版（C#）=="
if [ -f ./wage-test.exe ]; then
    values=$(./wage-test.exe 2>/dev/null | grep '^VALUE ')
    while IFS='|' read -r args expected; do
        [ -z "$args" ] && continue
        got=$(printf '%s\n' "$values" | awk -v want="$args" '{ if ($2 " " $3 " " $4 " " $5 == want) { print $6; exit } }')
        compare "csc $args" "$expected" "$got"
    done <<CS
$cases
CS
else
    echo "  跳过：没有 wage-test.exe"
fi

echo "== 非法输入 =="
if sh ./hourlywage.sh 0 0 0 0 >/dev/null 2>&1; then
    fail=$((fail + 1))
    echo "[FAIL] 终端版：总工时为 0 未被拒绝"
else
    echo "[PASS] 终端版：总工时为 0 被拒绝"
fi

echo
if [ "$fail" -eq 0 ]; then
    echo "三端结果一致。"
    exit 0
fi

echo "不一致或失败项：$fail"
exit 1
