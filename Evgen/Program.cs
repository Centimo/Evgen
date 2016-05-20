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
            Story[] Stories = new Story[40000];
            int cStories = 0;
            string path = Path.GetFullPath(@".\..\..\..\text.txt");

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using (var sr = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Stories[cStories].name = line;
                    Stories[cStories].test = sr.ReadLine();
                    Stories[cStories++].text = sr.ReadLine();
                }
            }

            //WriteLine(Stories[0].name);
            //WriteLine(Stories[0].test);
            //WriteLine(Stories[0].text);

            for (int i = 0; i < cStories; i++)
            {
                ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.English);

                string[] words = Stories[i].words;

                words = Stories[i].text.Split(
                    new char[] { ' ', '\t', '\n', ',', '/', '\\', '?', '!', '<', '>', '\'', '|', ':', ';', ')', '(', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                 StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < words.Length; j++)
                {
                    //if (word.Length <= 1)
                    //    continue;

                    //if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                    LemmatizeOne(lmtz, ref words[j]);
                }
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
    struct Story
    {
        public string name;
        public string test;
        public string text;
        public string[] words;// = new string[1000];
    }
}

