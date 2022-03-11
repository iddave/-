using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Morphology
{
    public class OneLetterDictionary
    {
        public List<string> KeyWords { get; set; } // нормализованное слово по которому будет осуществляться поиск
        //public Dictionary<HashSet<string>, string> AttributesDict { get; set; }
        public Dictionary<string, Dictionary<HashSet<string>, string>> WordVariations { get; set; }
        public OneLetterDictionary()
        {
            KeyWords = new List<string>(); //?
            //AttributesDict = new Dictionary<HashSet<string>, string>();
            WordVariations = new Dictionary<string, Dictionary<HashSet<string>, string>>();
        }

        public void AddWordVariation(HashSet<string> attributes, string variant)
        {
            var keyWord = KeyWords[KeyWords.Count - 1];
            WordVariations[keyWord].Add(attributes, variant); // нужно ли создавать новый сет
        }

        public string GetWordVariation(string keyWord, HashSet<string> attributes)
        {
            foreach(var variant in WordVariations[keyWord])
            {
                if (variant.Key.IsSubsetOf(attributes)) return variant.Value;
            }
            return keyWord;
        }
    }

    public class SentenceMorpher
    {
        public static OneLetterDictionary[] FullDict = new OneLetterDictionary[33];

        public static void InitFullDict()
        {
            for(int i =0; i<33; i++)
                FullDict[i] = new OneLetterDictionary();
        }

        private static bool IsStringValid(string s)
        {
            if (!string.IsNullOrEmpty(s))
                return char.IsLetter(s[0]);
            return false;
        }

        public static (HashSet<string>, string) ParseString(string line)
        {
            //в строках есть пробелы в конце
            var separators = new char[] { ' ', ',', '\t' };
            var strArr = line.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            var strArrLen = strArr.Length;
            var attributes = new HashSet<string>();
            for(int i = 1; i<strArrLen; i++)
                attributes.Add(strArr[i].ToLower());// tolower
            return (attributes, strArr[0]);
        }

        public static int GetIndex(char c)
        {
            return c == 'Ё' ? 32 : c - 'А';
        }

        public static void AddKeyWord(HashSet<string> attr, string keyWord, int ind)
        {
            FullDict[ind].KeyWords.Add(keyWord.ToUpper()); // toupper
            if(!FullDict[ind].WordVariations.ContainsKey(keyWord))
                FullDict[ind].WordVariations.Add(keyWord, new Dictionary<HashSet<string>, string>()); // empty dict
            FullDict[ind].AddWordVariation(attr, keyWord); // ключевое слово тоже в списке
        }

        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            bool lastLineWasNum = false;
            InitFullDict();
            string keyWord = "";
            foreach (var line in dictionaryLines)
            {
                var lineIsOk = IsStringValid(line);
                if (!lineIsOk)
                {
                    lastLineWasNum = true;
                    continue;
                }
                else
                {
                    var parseWord = ParseString(line);
                    if (lastLineWasNum)
                    {
                        var ind = GetIndex(parseWord.Item2.ToUpper()[0]);
                        AddKeyWord(parseWord.Item1, parseWord.Item2, ind);
                        lastLineWasNum = false;
                        keyWord = parseWord.Item2;
                    }
                    else
                    {
                        var ind = GetIndex(keyWord[0]);
                        FullDict[ind].AddWordVariation(parseWord.Item1, parseWord.Item2);
                    }
                }
            }
            return new SentenceMorpher();
        }

        public static string[] ParseInputSentence(string sentence)
        {
            return sentence.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static (string, HashSet<string>) ParseInputWord(string str)
        {
            var splitStr = str.Split('{', System.StringSplitOptions.RemoveEmptyEntries);
            var attributes = splitStr[1].TrimEnd('}').Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            return (splitStr[0], new HashSet<string>(attributes));
        }

        public static string FindWord(string keyWord, HashSet<string> attr)
        {
            keyWord = keyWord.ToUpper();
            var ind = GetIndex(keyWord[0]);
            return FullDict[ind].GetWordVariation(keyWord, attr);
        }

        public static bool NoAttributes(string s)
        {
            return !s.Contains('{');
        }

        /// <summary>
        ///     Выполняет склонение предложения согласно указанному формату
        /// </summary>
        /// <param name="sentence">
        ///     Входное предложение <para/>
        ///     Формат: набор слов, разделенных пробелами.
        ///     После слова может следовать спецификатор требуемой части речи (формат описан далее),
        ///     если он отсутствует - слово требуется перенести в выходное предложение без изменений.
        ///     Спецификатор имеет следующий формат: <code>{ЧАСТЬ РЕЧИ,аттрибут1,аттрибут2,..,аттрибутN}</code>
        ///     Если для спецификации найдётся несколько совпадений - используется первое из них
        /// </param>
        public virtual string Morph(string sentence)
        {
            var newSentence = new List<string>();
            foreach(var w in ParseInputSentence(sentence))
            {
                if (NoAttributes(w)) newSentence.Add(w.ToUpper());
                else
                {
                    var word = ParseInputWord(w);
                    newSentence.Add(FindWord(word.Item1.ToUpper(), word.Item2));
                }
            }
            return string.Join(' ',newSentence);
        }
    }
}
