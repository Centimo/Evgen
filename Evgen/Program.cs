using LemmaSharp;
using System;
using System.IO;
using System.Text;
using static System.Console;

namespace Evgen
{
    class Program
    {
        static void Main(string[] args)
        {
            //WriteLine(Directory.GetCurrentDirectory());
            //WriteLine(Environment.CurrentDirectory);
            //WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            //WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
            Parse();
        }

        static void Parse()
        {
            string path = Path.GetFullPath(@".\..\..\..\text.txt");
            string text;
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }

            text = text.Remove(10000);

            string[] words = text.Split(
                 new char[] { ' ', '\t', '\n', ',', '/', '\\', '?', '!', '<', '>', '\'', '|', ':', ';', ')', '(', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'},
                 StringSplitOptions.RemoveEmptyEntries);

            ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.English);
            for (int i = 0; i < words.Length; i++)
            foreach (string word in words)
            {
                //if (word.Length <= 1)
                //    continue;

                //if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                    LemmatizeOne(lmtz, ref words[i]);
            }
        }

        private static void LemmatizeOne(LemmaSharp.ILemmatizer lmtz, ref string word)
        {
            string wordLower = word.ToLower();
            word = lmtz.Lemmatize(wordLower);
            ForegroundColor = wordLower == word ? ConsoleColor.White : ConsoleColor.Red;
            WriteLine("{0,20} ==> {1}", word, word);
        }
    }

}

