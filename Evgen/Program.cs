using LemmaSharp;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static System.Console;

namespace Evgen
{
    class Program
    {
        int cStories = 0;
        Story[] Stories = new Story[40000];

        static void Main(string[] args)
        {
            //WriteLine(Directory.GetCurrentDirectory());
            //WriteLine(Environment.CurrentDirectory);
            //WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            //WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
            Program p = new Program();
        }

        public Program()
        {
            Parse();
            // тестовое ограничение - убрать в финале
            cStories = 200;

            for (int i = 0; i < cStories; i++)
            {
                // почему в одном месте надо ref, а в другом нет???
                Lemmatize(ref Stories[i]);
                DoFreq(Stories[i]);
            }


            Story ss = new Story();
            for (int i = 0; i < cStories; i++)
            {
                //if (Stories[i].name != "earn")
                //    continue;

                ss.text += Stories[i].text;
            }

            Lemmatize(ref ss);
            DoFreq(ss);

            Array.Sort(ss.freq, ss.words);
            for (int j = ss.words.Length - 1; j > ss.words.Length - 30; j--)
            {
                Write("{0, 12}   ", ss.words[j]);
                WriteLine(ss.freq[j]);
            }
            WriteLine();


            //for (int i = 0; i < 10000; i++)
            //{
            //    Array.Sort(Stories[i].freq, Stories[i].words);
            //    WriteLine("story {0}", i);
            //    for (int j = Stories[i].words.Length - 1; j > Stories[i].words.Length - 10; j--)
            //    {
            //        Write("{0, 12}   ", Stories[i].words[j]);
            //        WriteLine(Stories[i].freq[j]);
            //    }
            //    WriteLine();
            //}

            //int c = 0;
            //for (int i = 0; i < Stories[c].words.Length; i++)
            //{
            //    Write("{0, 10}   ", Stories[c].words[i]);
            //    WriteLine(Stories[c].freq[i]);
            //}
        }
        
        void Parse()
        {
            string path = Path.GetFullPath(@".\..\..\..\text.txt");

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using (var sr = new StreamReader(fileStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != "wheat")
                        continue;
                    Stories[cStories].name = line;
                    Stories[cStories].test = sr.ReadLine();
                    Stories[cStories++].text = sr.ReadLine();
                    sr.ReadLine();
                }
            }
        }
        void Lemmatize(ref Story story)
        {
            ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.English);

            story.words = story.text.Split(
                new char[] { ' ', '\t', '\n', ',', '/', '\\', '?', '!', '<', '>', '\'', '|', ':', ';', ')', '(', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                StringSplitOptions.RemoveEmptyEntries);

            story.freq = new int[story.words.Length];
            
            for (int j = 0; j < story.words.Length; j++)
                LemmatizeOne(lmtz, ref story.words[j]);
        }
        void DoFreq(Story story)
        {
            string[] words = story.words;
            int[] freq = story.freq;

            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].TrimEnd('.');

                if (words[i].Length < 3 || !Regex.IsMatch(words[i], @"^[a-zA-Z]+$"))
                    continue;

                bool finded = false;
                int k = 0;
                while (!finded && k < i)
                {
                    if (words[i] == words[k])
                    {
                        freq[k]++;
                        finded = true;
                    }
                    k++;
                }

                if (!finded)
                    freq[i]++;
            }
        }
        void LemmatizeOne(LemmaSharp.ILemmatizer lmtz, ref string word)
        {
            string wordLower = word.ToLower();
            word = lmtz.Lemmatize(wordLower);

            ForegroundColor = wordLower == word ?
                ConsoleColor.White : ConsoleColor.Red;
            //WriteLine("{0,20} ==> {1}", word, word);
        }

        struct Story
        {
            public string name;
            public string test;
            public string text;
            public string[] words;// = new string[1000];
            public int[] freq;
        }
    }
}
