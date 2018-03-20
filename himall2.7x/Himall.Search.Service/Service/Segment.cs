using Himall.Search.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpICTCLAS;
using System.Configuration;
using System.IO;


namespace Himall.Search.Service
{
    public class Segment : ISegment
    {
        private static WordSegment wordSegment = null;

        private static string dictPath = null;

        public Segment(string dictpath)
        {
            if (wordSegment == null)
            {
                wordSegment = new WordSegment();
                dictPath = dictpath;
                wordSegment.InitWordSegment(dictpath);
                wordSegment.PersonRecognition = false;
                wordSegment.TransPersonRecognition = false;
                wordSegment.PlaceRecognition = false;
            }
        }

        /// <summary>
        /// 添加新分词
        /// </summary>
        /// <param name="words"></param>
        //public void AddNewWord(List<string> words)
        //{
        //    //string DictPath = Path.Combine(Environment.CurrentDirectory, "Data") +
        //    //             Path.DirectorySeparatorChar;

        //    WordDictionary dict = new WordDictionary();
        //    var dic = Core.Helper.IOHelper.GetMapPath("~/App_Data/Dict/");
        //    dict.Load(dic + "coreDict.dct");
        //    foreach (var word in words)
        //    {
        //        dict.AddItem(word, Utility.GetPOSValue("n"), 10);
        //    }
        //    dict.Save(dic + "coreDictNew.dct");
        //}

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="str">需分词的文本</param>
        /// <returns>分词结果</returns>
        public List<string> DoSegment(string str)
        {
            str = str.Trim();
            List<string> result = new List<string>();
            var r = wordSegment.Segment(str);
            for (int i = 0; i < r.Count; i++)
            {
                for (int j = 1; j < r[i].Length - 1; j++)
                {
                    if (result.IndexOf(r[i][j].sWord.ToLower()) == -1)
                    {
                        if (!result.Contains(r[i][j].sWord.ToLower()))
                            result.Add(r[i][j].sWord.ToLower());
                    }
                }
            }
            return result;
        }
    }
}
