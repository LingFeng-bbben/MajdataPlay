using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Utils
{
    public static class SimaiMirror
    {
        private static readonly Dictionary<char, char> MIRROR_LEFT_RIGHT_MAP = new()
        {
            { '1', '8' },
            { '2', '7' },
            { '3', '6' },
            { '4', '5' },
            { '5', '4' },
            { '6', '3' },
            { '7', '2' },
            { '8', '1' },
            { 'q', 'p' },
            { 'p', 'q' },
            { '<', '>' },
            { '>', '<' },
            { 'z', 's' },
            { 's', 'z' }
        };

        // 当遇到这些字符时 使用特殊的映射表
        private static readonly HashSet<char> MIRROR_SPECIAL_PREFIX = new() { 'D', 'E' };

        private static readonly Dictionary<char, char> MIRROR_LEFT_RIGHT_SPECIAL_MAP = new()
        {
            { '8', '2' },
            { '2', '8' },
            { '3', '7' },
            { '7', '3' },
            { '4', '6' },
            { '6', '4' },
            { '1', '1' },
            { '5', '5' }
        };

        private static readonly Dictionary<char, char> MIRROR_UPSIDE_DOWN_MAP = new()
        {
            { '4', '1' },
            { '5', '8' },
            { '6', '7' },
            { '3', '2' },
            { '7', '6' },
            { '2', '3' },
            { '8', '5' },
            { '1', '4' },
            { 'q', 'p' },
            { 'p', 'q' },
            { 'z', 's' },
            { 's', 'z' }
        };

        private static readonly Dictionary<char, char> MIRROR_UPSIDE_DOWN_SPECIAL_MAP = new()
        {
            { '4', '2' },
            { '2', '4' },
            { '1', '5' },
            { '5', '1' },
            { '8', '6' },
            { '6', '8' },
            { '3', '3' },
            { '7', '7' }
        };

        private static readonly Dictionary<char, char> ROTATE_CW_45_MAP = new()
        {
            { '8', '1' },
            { '7', '8' },
            { '6', '7' },
            { '5', '6' },
            { '4', '5' },
            { '3', '4' },
            { '2', '3' },
            { '1', '2' }
        };

        private static readonly Dictionary<char, char> ROTATE_CCW_45_MAP = new()
        {
            { '1', '8' },
            { '2', '1' },
            { '3', '2' },
            { '4', '3' },
            { '5', '4' },
            { '6', '5' },
            { '7', '6' },
            { '8', '7' }
        };

        private static readonly HashSet<char> ROTATE_CW_45_SPECIAL_PREFIX = new() { '2', '6' };

        private static readonly HashSet<char> ROTATE_CCW_45_SPECIAL_PREFIX = new() { '3', '7' };

        private static readonly Dictionary<char, char> ROTATE_45_SPECIAL_MAP = new()
        {
            { '<', '>' },
            { '>', '<' }
        };

        private static readonly string HS_SEQUENCE = "<HS*";

        public static string NoteMirrorHandle(string str, MirrorType type)
        {
            // NOTE: 类似 1-5[8:1]{16}, 这样的字符串 可以被SimaiProcess处理 但无法被正确镜像
            // 我认为这是对的 因为这种语法本身就是错误的 只不过SimaiProcess没有做处理而已 不能因此而妥协 以上

            StringBuilder resultString = new StringBuilder();   // 最终的结果
            StringBuilder curPart = new StringBuilder();        // 当前的一部分
            bool isPartIgnored = false;     // 当前部分是否需要被忽略
            int hsStatus = 0;              // 当前部分是否是HS语法的一部分 0-未检测到 1-检测到< 2-检测到H 3-检测到S 4-检测到*  必须按照01234的顺序走完才算HS 否则不算

            // 空白字符会被正常加入每一个part 因此应当在子方法中进行忽略处理 这是为了保持空白字符位置不变
            foreach (char c in str)
            {
                curPart.Append(c);

                // 如果存在以下字符 则本部分一定是被忽略的
                if (!isPartIgnored && (c == '{' || c == '}' || c == '(' || c == ')'))
                {
                    isPartIgnored = true;
                }

                if (hsStatus == 0)
                {
                    if (HS_SEQUENCE[0] == c)
                    {
                        // 检查到了疑似HS语法的开头 开始检查接下来的字符
                        hsStatus = 1;
                    }
                }
                else if (hsStatus != HS_SEQUENCE.Length)
                {
                    // 如果hsStatus不是0 说明已经开始检查了 那么接下来就必须严格紧凑地检查到剩余的HS语法字符
                    // 当然 空白字符不算
                    if (!char.IsWhiteSpace(c))
                    {
                        if (HS_SEQUENCE[hsStatus] == c)
                        {
                            // 符合 则步进
                            hsStatus++;
                            if (hsStatus == HS_SEQUENCE.Length)
                            {
                                // 检查到结尾了 则肯定是一个hs结构了
                                isPartIgnored = true;
                            }
                        }
                        else
                        {
                            // 不符合 说明这不是一个HS结构 则归零重新开始探索
                            hsStatus = 0;
                        }
                    }
                }

                // 以下字符表示本part结束
                if (c == '}' || c == ')' || c == ',' || c == '/' || c == '`' ||
                    hsStatus == 4 && c == '>')
                {
                    if (isPartIgnored)
                    {
                        // 需要忽略的段落 直接原样加进去就行了
                        resultString.Append(curPart.ToString());
                    }
                    else
                    {
                        // 需要镜像计算的段落 调用子方法进行转换
                        resultString.Append(NoteMirrorPart(curPart.ToString(), type));
                    }

                    isPartIgnored = false;
                    hsStatus = 0;
                    curPart.Clear();
                }
            }

            // 处理完以后可能还会有剩余的没做转换（可能结尾的逗号没选进去） 也要处理一下
            if (curPart.Length > 0)
            {
                if (isPartIgnored)
                {
                    // 需要忽略的段落 直接原样加进去就行了
                    resultString.Append(curPart.ToString());
                }
                else
                {
                    // 需要镜像计算的段落 调用子方法进行转换
                    resultString.Append(NoteMirrorPart(curPart.ToString(), type));
                }
            }

            return resultString.ToString();
        }

        private static string NoteMirrorPart(string str, MirrorType type)
        {
            switch (type)
            {
                case MirrorType.LRMirror:
                    str = NormalMirrorPart(str, MIRROR_LEFT_RIGHT_MAP, MIRROR_LEFT_RIGHT_SPECIAL_MAP, MIRROR_SPECIAL_PREFIX);
                    break;
                case MirrorType.UDMirror:
                    str = NormalMirrorPart(str, MIRROR_UPSIDE_DOWN_MAP, MIRROR_UPSIDE_DOWN_SPECIAL_MAP, MIRROR_SPECIAL_PREFIX);
                    break;
            }

            return str;
        }

        private static string NormalMirrorPart(string str, Dictionary<char, char> normalMap, Dictionary<char, char> specialMap, HashSet<char> specialPrefix)
        {
            StringBuilder result = new StringBuilder();
            bool isSpecialPrefix = false;
            bool isInBracket = false;

            foreach (char c in str)
            {
                // 空白字符忽略
                if (char.IsWhiteSpace(c))
                {
                    result.Append(c);
                    continue;
                }

                // 以下是处理字符的部分
                if (isInBracket || c == '[')
                {
                    // 进入方括号时忽略 因为这里是时长之类的设置
                    // 注意当前字符为'['的时候也进入此分支 因为此时isInBrancket还是false 需要等到下面那个if块才会变
                    result.Append(c);
                }
                else if (isSpecialPrefix)
                {
                    // 如果前面读到一个D或者E 则使用特殊的映射
                    isSpecialPrefix = false;
                    if (specialMap.ContainsKey(c))
                    {
                        result.Append(specialMap[c]);
                    }
                    else if (int.TryParse(c.ToString(), out int i) && normalMap.ContainsKey(c))
                    {
                        result.Append(normalMap[c]);
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
                else
                {
                    // 其他情况 则使用默认映射
                    if (normalMap.ContainsKey(c))
                    {
                        result.Append(normalMap[c]);
                    }
                    else
                    {
                        result.Append(c);
                    }
                }

                // 以下是记录状态的部分
                // 记录是否进入了方括号内（即时间）
                if (c == '[')
                {
                    isInBracket = true;
                }
                else if (c == ']')
                {
                    isInBracket = false;
                }
                // 记录是否是一个特殊前缀 若是 则下面的字符需要使用特别的映射
                // 在左右或上下镜像中，用于D、E区Touch的特殊处理
                // 在45旋转中，本意是对Simai的弱智">"，"<"进行特殊处理，但没有判断本Note是否为Tap
                if (specialPrefix.Contains(c))
                {
                    isSpecialPrefix = true;
                }
            }

            return result.ToString();
        }
    }
}
