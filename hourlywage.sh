#!/bin/sh
# 时薪计算器 —— Linux / macOS 终端版
#
# 时薪 =（月薪 + 奖金）÷（每日工时 × 21.75 + 节假日总工时）
# 月计薪天数取年均值：（365 − 104）÷ 12 = 21.75 天/月
#
# 只依赖 sh 与 awk，不需要装任何东西。

PAY_DAYS_PER_MONTH=21.75

usage() {
    cat <<'EOF'
时薪计算器 —— 时薪 =（月薪 + 奖金）÷（每日工时 × 21.75 + 节假日总工时）

用法：
  ./hourlywage.sh 月薪 每日工时 节假日总工时 奖金
  ./hourlywage.sh            交互输入
  ./hourlywage.sh -h         显示本帮助

示例：
  ./hourlywage.sh 8000 8 0 0     → 45.98
EOF
}

# 参数为空串时按 0 处理，方便交互时直接回车
calc() {
    awk -v s="$1" -v h="$2" -v o="$3" -v b="$4" -v days="$PAY_DAYS_PER_MONTH" '
        function isnum(x) {
            return x ~ /^-?[0-9]+(\.[0-9]+)?$/ || x ~ /^-?\.[0-9]+$/
        }
        BEGIN {
            if (s == "") s = 0
            if (h == "") h = 0
            if (o == "") o = 0
            if (b == "") b = 0
            if (!isnum(s) || !isnum(h) || !isnum(o) || !isnum(b)) {
                print "错误：请输入数字" > "/dev/stderr"
                exit 2
            }
            if (s < 0 || h < 0 || o < 0 || b < 0) {
                print "错误：请输入不小于 0 的数字" > "/dev/stderr"
                exit 2
            }
            total = h * days + o
            if (total <= 0) {
                print "错误：总工时不能为 0" > "/dev/stderr"
                exit 2
            }
            value = (s + b) / total
            printf "%.2f\n", int(value * 100 + 0.5) / 100
        }'
}

ask() {
    printf '%s [%s]: ' "$1" "$2"
    IFS= read -r REPLY || REPLY=""
    if [ -z "$REPLY" ]; then
        REPLY="$2"
    fi
    return 0
}

main() {
    case "$#" in
        0)
            ask "月薪（元）" 8000
            salary="$REPLY"
            ask "每日工时（小时）" 8
            hours="$REPLY"
            ask "节假日总工时（小时）" 0
            holiday="$REPLY"
            ask "奖金（元）" 0
            bonus="$REPLY"
            result=$(calc "$salary" "$hours" "$holiday" "$bonus") || exit $?
            printf '时薪：%s 元/小时\n' "$result"
            ;;
        1)
            case "$1" in
                -h|--help) usage; exit 0 ;;
                *) usage; exit 2 ;;
            esac
            ;;
        4)
            calc "$1" "$2" "$3" "$4" || exit $?
            ;;
        *)
            usage
            exit 2
            ;;
    esac
}

main "$@"
