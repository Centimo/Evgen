using LemmaSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;

namespace Evgen
{
	public class SomeTupleComparer : IComparer<Tuple<double, int[]>>
	{
		public int Compare(Tuple<double, int[]> x, Tuple<double, int[]> y)
		{
			return x.Item1.CompareTo(y.Item1);
		}
	}

	public class SomeReverseTupleComparer : IComparer<Tuple<double, int[]>>
	{
		public int Compare(Tuple<double, int[]> x, Tuple<double, int[]> y)
		{
			return y.Item1.CompareTo(x.Item1);
		}
	}

	class Program
	{
		SortedDictionary<string, List<Story>> _test_texts = new SortedDictionary<string, List<Story>>();
		SortedDictionary<string, List<Story>> _train_texts = new SortedDictionary<string, List<Story>>();
		SortedDictionary<string, SortedDictionary<string, double>> _themes_freq =
			new SortedDictionary<string, SortedDictionary<string, double>>();

		SortedDictionary<string, Vector<double>> _words_operators = new SortedDictionary<string, Vector<double>>();

		SortedDictionary<string, uint> _stopwords = new SortedDictionary<string, uint>();

		List<string> _themes_names = new List<string>();
		SortedDictionary<string, List<int>> _themes =
			new SortedDictionary<string, List<int>>();

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
			ILemmatizer lmtz = new LemmatizerPrebuiltCompact(LemmaSharp.LanguagePrebuilt.English);
			init_stopwords();
			parse();

			foreach (var theme in _train_texts)
			{
				_themes_names.Add(theme.Key);
				foreach (Story story in theme.Value)
				{
					lemmatize(story, lmtz);
				}

				var temp_freq = new SortedDictionary<string, double>();
				double words_count = 0;
				foreach (Story story in theme.Value)
				{
					words_count += story._words.Count;
					double val = 0;
					foreach (var word_freq in story._freq)
					{
						if (temp_freq.TryGetValue(word_freq.Key, out val))
						{
							temp_freq[word_freq.Key] += word_freq.Value;
						}
						else
						{
							temp_freq[word_freq.Key] = word_freq.Value;
						}
					}
				}

				var keys = new List<string>(temp_freq.Keys);
				foreach (var word_freq in keys)
				{
					temp_freq[word_freq] = temp_freq[word_freq] / words_count;
				}

				_themes_freq.Add(theme.Key, temp_freq);
			}

			foreach (var theme in _test_texts)
			{
				foreach (Story story in theme.Value)
				{
					lemmatize(story, lmtz);
				}
			}


			string path = Path.GetFullPath(@".\..\..\..\test_otchet11.txt");
			var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
			var sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
			List<Tuple<double, int[]>> exps = new List<Tuple<double, int[]>>();
			for (int i = 0; i < 10; ++i)
			{
				for (int j = 0; j < 10; ++j)
				{
					for (int k = 0; k < 10; ++k)
					{
						train(0.5, 6000, -5 + i, -5 + j, -5 + k);
						test(_test_texts);

						sw.WriteLine((-5 + i).ToString() + " " + (-5 + j).ToString() + " " + (-5 + k).ToString());

						double recall_deviation = 0;
						double precision_deviation = 0;
						foreach (var theme in _themes)
						{
							double recall = get_recall(theme.Value[1], theme.Value[2]);
							double precision = get_precision(theme.Value[1], theme.Value[3]);
							recall_deviation += Math.Pow((1 - recall)*100, 2);
							precision_deviation += Math.Pow((1 - precision) * 100, 2);
							sw.WriteLine(theme.Key.ToString() + " " + recall.ToString() + " " + precision.ToString());
						}

						sw.WriteLine(recall_deviation);
						sw.WriteLine(precision_deviation);
						exps.Add(Tuple.Create(recall_deviation, new int[3] { i, j, k }));
						sw.WriteLine();

						_themes = new SortedDictionary<string, List<int>>();
						_words_operators = new SortedDictionary<string, Vector<double>>();
						// Console.WriteLine("done");
					}
				}
			}

			int l = 0;
			SomeTupleComparer stc = new SomeTupleComparer();
			exps.Sort(stc);
			foreach (var exp in exps)
			{
				if(l > 50)
				{
					break;
				}

				sw.WriteLine(exp.Item2[0].ToString() + " " + exp.Item2[1].ToString() + " " + exp.Item2[2].ToString() + " " + exp.Item1.ToString());
				++l;
			}

			l = 0;
			SomeReverseTupleComparer srtc = new SomeReverseTupleComparer();
			exps.Sort(srtc);
			foreach (var exp in exps)
			{
				if (l > 50)
				{
					break;
				}

				sw.WriteLine(exp.Item2[0].ToString() + " " + exp.Item2[1].ToString() + " " + exp.Item2[2].ToString() + " " + exp.Item1.ToString());
				++l;
			}

			sw.WriteLine();
			sw.Close();
		}

		double get_recall(int true_count, int false_count)
		{
			return (double) true_count / (true_count + false_count + 1);
		}

		double get_precision(int true_count, int another_count)
		{
			return (double)true_count / (another_count + 1);
		}

		void train(double max_add_multiplyer, int max_words, double power_word_freq, double power_word_max_freq, double power_max_freq)
		{
			int i = 0;
			foreach (var theme in _train_texts)
			{
				_themes.Add(theme.Key, new List<int>(4));
				_themes[theme.Key].Add(i);
				_themes[theme.Key].Add(0);
				_themes[theme.Key].Add(0);
				_themes[theme.Key].Add(0);
				++i;
			}

			int themes_count = _test_texts.Count;
			SortedDictionary<string, uint> checked_words = new SortedDictionary<string, uint>();

			SortedDictionary<string, Tuple<double, string>> most_freq_words =
				new SortedDictionary<string, Tuple<double, string>>();

			// Получаю список всех слов с указанием их наибольшей плотности и соотв. темы

			foreach (var theme_freq in _themes_freq)
			{
				Tuple<double, string> val;
				foreach (var word_freq in theme_freq.Value)
				{
					if (most_freq_words.TryGetValue(word_freq.Key, out val))
					{
						if (val.Item1 < word_freq.Value)
						{
							most_freq_words[word_freq.Key] = Tuple.Create(word_freq.Value, word_freq.Key);
						}
					}
					else
					{
						most_freq_words[word_freq.Key] = Tuple.Create(word_freq.Value, word_freq.Key);
					}
				}
			}

			// генерируем операторы
			var sorted_words_freqs = most_freq_words.Values.OrderBy(Tuple => -Tuple.Item1);
			double max_freq = sorted_words_freqs.ElementAt(0).Item1;

			if (max_words == 0)
			{
				max_words = most_freq_words.Count;
			}

			int words_count = 0;
			foreach (var word_freq in sorted_words_freqs)
			{
				if (words_count == max_words)
				{
					break;
				}

				if (checked_words.ContainsKey(word_freq.Item2))
				{
					continue;
				}
				double val;
				Vector<double> multiplyers = Vector<double>.Build.Dense(themes_count, 1);
				int m = 0;
				foreach (var theme_freq in _themes_freq)
				{
					theme_freq.Value.TryGetValue(word_freq.Item2, out val);
					
					multiplyers[m] += Math.Pow(val, power_word_freq)* 
						Math.Pow(word_freq.Item1, power_word_max_freq) *
						Math.Pow(max_freq, power_max_freq) * max_add_multiplyer;

					++m;
				}
				_words_operators.Add(word_freq.Item2, multiplyers);
				checked_words.Add(word_freq.Item2, 0);
				++words_count;
			}
		}
		void parse()
		{
			string path = Path.GetFullPath(@".\..\..\..\text.txt");

			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			using (var sr = new StreamReader(fileStream, System.Text.Encoding.UTF8))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					List<Story> val;
					switch (sr.ReadLine())
					{
						case "t":
							if (_train_texts.TryGetValue(line, out val))
							{
								_train_texts[line].Add(new Story(sr.ReadLine()));
							}
							else
							{
								_train_texts[line] = new List<Story>() { new Story(sr.ReadLine()) };
							}
							break;

						case "f":
							if (_test_texts.TryGetValue(line, out val))
							{
								_test_texts[line].Add(new Story(sr.ReadLine()));
							}
							else
							{
								_test_texts[line] = new List<Story>() { new Story(sr.ReadLine()) };
							}
							break;

						default:
							break;
					}

					sr.ReadLine();
				}
			}
		}

		void lemmatize(Story story, ILemmatizer lmtz)
		{
			string[] words = story._text.Split(
					new char[] { ' ', '.', '\t', '\n', ',', '/', '\\', '?', '!', '<', '>', '\'', '"', '&', '[', ']',
							'|', ':', ';', ')', '(', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '*' },
					StringSplitOptions.RemoveEmptyEntries);

			uint val = 0;
			for (uint i = 0; i < words.Length; i++)
			{
				words[i] = words[i].ToLower();
				words[i] = lmtz.Lemmatize(words[i]);

				if (/*!_stopwords.ContainsKey(words[i]) &&*/ words[i].Length > 2)
				{
					story._words.Add(words[i]);

					if (story._freq.TryGetValue(words[i], out val))
					{
						story._freq[words[i]] += 1;
					}
					else
					{
						story._freq[words[i]] = 1;
					}
				}
			}
		}

		void init_stopwords()
		{
			_stopwords.Add("this", 0);
			_stopwords.Add("the", 0);
			_stopwords.Add("are", 0);
			_stopwords.Add("april", 0);
			_stopwords.Add("march", 0);
			_stopwords.Add("and", 0);
			//_stopwords.Add("", 0);
		}

		void test(SortedDictionary<string, List<Story>> texts)
		{
			foreach (var theme in texts)
			{
				foreach (var story in theme.Value)
				{
					get_story_theme(story, theme.Key);
				}
			}
		}

		void operate(ref Vector<double> current, Vector<double> modifier)
		{
			for (int i = 0; i < current.Count && i < modifier.Count; ++i)
			{
				current[i] *= modifier[i];
			}

			// Нормализируем
			current = current.Normalize(2);
		}

		void get_story_theme(Story story, string theme)
		{
			Vector<double> start = Vector<double>.Build.Dense(_themes_freq.Count, 1);
			Vector<double> val = Vector<double>.Build.Dense(_themes_freq.Count, 1);
			start = start.Normalize(2);
			foreach (var word in story._words)
			{
				if (_words_operators.TryGetValue(word, out val))
				{
					operate(ref start, _words_operators[word]);
				}
			}

			int index = start.MaximumIndex();
			_themes[_themes_names[index]][3] += 1;
			if (_themes[theme][0] == index)
			{
				_themes[theme][1] += 1;
			}
			else
			{
				_themes[theme][2] += 1;
			}
		}

		struct Story
		{
			public string _text;
			public List<string> _words;
			public SortedDictionary<string, uint> _freq;

			public Story(string text)
			{
				_text = text;
				_words = new List<string>();
				_freq = new SortedDictionary<string, uint>();
			}
		}
	}
}
