using LemmaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static System.Console;

namespace Evgen
{
	class Program
	{
		SortedDictionary<string, List<Story>> _test_texts;
		SortedDictionary<string, List<Story>> _train_texts;
		SortedDictionary<string, uint> _stopwords;

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

			foreach (var theme in _train_texts)
			{
				for (int i = 0; i < theme.Value.Count; i++)
				{
					Lemmatize(theme.Value[i]);
				}
			}

			WriteLine();
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
					switch (sr.ReadLine())
					{
						case "t":
							_train_texts[line].Add(new Story(sr.ReadLine()));
							break;

						case "f":
							_train_texts[line].Add(new Story(sr.ReadLine()));
							break;

						default:
							break;
					}

					sr.ReadLine();
				}
			}
		}
		void Lemmatize(Story story)
		{
			ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.English);

			string[] words = story._text.Split(
					new char[] { ' ', '.', '\t', '\n', ',', '/', '\\', '?', '!', '<', '>', '\'',
						'|', ':', ';', ')', '(', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
					StringSplitOptions.RemoveEmptyEntries);

			for (uint i = 0; i < words.Length; i++)
			{
				words[i] = words[i].ToLower();
				words[i] = lmtz.Lemmatize(words[i]);

				if (_stopwords.ContainsKey(words[i]))
				{
					story._words.Add(words[i]);
					story._frec[words[i]] += 1;
				}
			}
		}

		struct Story
		{
			public string _text;
			public List<string> _words;
			public SortedDictionary<string, uint> _frec;

			public Story(string text)
			{
				_text = text;
				_words = new List<string>();
				_frec = new SortedDictionary<string, uint>();
			}
		}
	}
}
