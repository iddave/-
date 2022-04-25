using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 
namespace Morphology
{
    public class SentenceMorpher
    {
        // public static List<string> KeyWords = new List<string>(); // нормализованное слово по которому будет осуществляться поиск
        public static Dictionary<string, List<(HashSet<string>, string)>> WordVariations = new Dictionary<string, List<(HashSet<string>, string)>>();
 
        private static bool IsStringValid(string s)
        {
            int num;
            return !string.IsNullOrEmpty(s) && !int.TryParse(s.Trim(), out num);
        }
 
        public static (HashSet<string>, string) ParseString(string line)
        {
            //в строках есть пробелы в конце
            var separators = new char[] { ' ', ',', '\t' };
            var strArr = line.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            var strArrLen = strArr.Length;
            var attributes = new HashSet<string>();
            for (int i = 1; i < strArrLen; i++)
                attributes.Add(strArr[i].ToLower());
            return (attributes, strArr[0].ToUpper());
        }
 
        public static void AddWordVariation(HashSet<string> attributes, string wordForm, string keyWord)
        {
            var variant = (attributes, wordForm);
            WordVariations[keyWord].Add(variant);
        }
 
        public static void AddKeyWord(HashSet<string> attributes, string keyWord)
        {
 
            if (!WordVariations.ContainsKey(keyWord))
                WordVariations.Add(keyWord, new List<(HashSet<string>, string)>()); // empty List
            AddWordVariation(attributes, keyWord, keyWord); // ключевое слово тоже в списке
        }
 
        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            bool lastLineWasNum = false;
            string keyWord = "";
            foreach (var line in dictionaryLines)
            {
                var lineIsOk = IsStringValid(line);
                if (!lineIsOk)
                {
                    lastLineWasNum = true;
                }
                else
                {
                    var parseWord = ParseString(line);
                    var currWord = parseWord.Item2;
                    var currAttr = parseWord.Item1;
 
                    if (lastLineWasNum)
                    {
                        keyWord = currWord;
                        AddKeyWord(currAttr, keyWord);
                        lastLineWasNum = false;
                    }
                    else
                    {
                        AddWordVariation(currAttr, currWord, keyWord);
                    }
                }
            }
            return new SentenceMorpher();
        }
 
        public static string FindWord(string keyWord, HashSet<string> attributes)
        {
            if (!WordVariations.ContainsKey(keyWord)) return keyWord;
            foreach (var variant in WordVariations[keyWord])
            {
                if (variant.Item1.IsSupersetOf(attributes)) return variant.Item2;
            }
            return keyWord;
        }
 
        public static HashSet<string> ConvertToHashSet(string s)
        {
            s = s.TrimStart('{').TrimEnd('}');
            var set = new HashSet<string>();
            foreach (var gramem in s.Split(new[] { ',',' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                set.Add(gramem.ToLower());
            return set;
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
 
            sentence = sentence.TrimEnd() + " "; //если вдруг на конце уже были пробелы
            var pattern = @"(.+?)(\{.*?\})?\s";
            foreach (Match match in Regex.Matches(sentence, pattern))
            {
                var keyWord = match.Groups[1].Value.ToUpper(); // тут должно быть слово
                var attributesStr = match.Groups[2].Value; // тут атрибуты
                if (string.IsNullOrWhiteSpace(attributesStr) || attributesStr == "{}") 
                    newSentence.Add(keyWord);
                else
                {
                    var attributes = ConvertToHashSet(attributesStr);
                    newSentence.Add(FindWord(keyWord, attributes));
                }
            }
            return string.Join(' ', newSentence);
        }
    }
}
 
