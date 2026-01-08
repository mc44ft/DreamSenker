using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

namespace PlayArk.DialogueSystem.Runtime
{
    public class DialogueTextPreprocessor : ITextPreprocessor
    {
        #region 正则表达式
        // @可以取消转移字符
        // .代表 任意字符
        // *代表 匹配前者零次或多次
        // ?写在另一个量词后面时 会将前一个量词从默认的贪婪模式转换为非贪婪模式 其结果就是 匹配最短的结果 < b >< i > 而不是< b><i >
        // ^ 匹配输入字符串的开始位置
        // \d代表十进制数字
        // +代表前一个字符出现一次或多次
        // ?代表 前一个字符出现零次或一次
        // .代表 任意字符  \.代表一个点
        // |代表或者
        #endregion
        /// <summary>
        /// 记录停顿位置的字典
        /// key 停顿位置 value 停顿时间
        /// </summary>
        public Dictionary<int, float> IntervalDict = new Dictionary<int, float>();
        public List<RubyData> RubyDataList = new List<RubyData>();
        public string PreprocessText(string text)
        {
            IntervalDict.Clear();
            RubyDataList.Clear();

            string processingText = text;

            string pattern = "<.*?>";

            Match match = Regex.Match(processingText, pattern);
            while (match.Success)
            {
                //去掉两侧的尖括号
                string label = match.Value.Substring(1, match.Value.Length - 2);

                //如果可以成功转换为浮点型小数
                if (float.TryParse(label, out float result))
                {
                    //记录停顿信息
                    if(match.Index - 1 < 0)
                    {
                        //如果 <数字> 写在开头 那这里的索引就为负数了，这里直接让索引0开始停顿
                        IntervalDict[0] = result;
                    }
                    else
                    {
                        IntervalDict[match.Index - 1] = result;
                    }
                        
                }

                //注音判断
                else if (Regex.IsMatch(label, "^r=.+"))
                {
                    RubyDataList.Add(new RubyData(match.Index, label.Substring(2)));
                }
                else if(Regex.IsMatch(label, "/r"))
                {
                    if(RubyDataList.Count > 0)
                    {
                        RubyDataList[RubyDataList.Count - 1].EndIndex = match.Index - 1;
                    }
                }

                //删除处理后的标签
                processingText = processingText.Remove(match.Index, match.Length);

                //继续匹配
                match = Regex.Match(processingText, pattern);
            }

            //还原文本内容
            processingText = text;

            //删除<自定义>内容
            pattern = @"(<(\d+)(\.\d+)?>)|(</r>)|(<r=.*?>)";
            processingText = Regex.Replace(processingText, pattern, "");
            return processingText;

        }
        public bool TryGetRubyFromStart(int index, out RubyData rubyData)
        {
            rubyData = null;
            foreach (RubyData data in RubyDataList)
            {
                if(data.StartIndex == index)
                {
                    rubyData = data;
                    return true;
                }
            }
            return false;
        }
    }

    public class RubyData
    {
        public int StartIndex;
        public int EndIndex;
        public string RubyContent;
        public RubyData(int startIndex, string rubyContent)
        {
            StartIndex = startIndex;
            //这里将EndIndex也设置为StartIndex 因为EndIndex后续会再次赋值
            EndIndex = startIndex;

            RubyContent = rubyContent;
        }
    }
}