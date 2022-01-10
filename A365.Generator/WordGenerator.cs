using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A365.Generator
{
    public class WordGenerator
    {
        private List<int> _pattern = new List<int>();
        private string[] _dict = new string[] { "accept", "barefoot", "coast", "define", "enable", "fault", "glass",
                                                    "handle", "include", "kernel", "leak", "match", "network", "offset",
                                                    "patch", "quality", "rate", "salt", "tag", "unique", "visible", "weak",
                                                    "yield", "zookeeper"};
        private string _separtor = ". ";
        private int _maxValue;
        private StringBuilder _stringBuilder;
        private Random _random;
        private List<string> _duplicate;

        public WordGenerator()
        {
            _maxValue = _dict.Length - 1;
            _stringBuilder = new StringBuilder();
            _duplicate = new List<string>();

            _random = new Random();

            for (int i = _dict.Length - 1; i > 0; i--)
            {
                int swapIndex = _random.Next(i + 1);
                var tmp = _dict[i];
                _dict[i] = _dict[swapIndex];
                _dict[swapIndex] = tmp;
            }

            _pattern.Add(0);
        }

        public string Next()
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(_random.Next(0, int.MaxValue));
            _stringBuilder.Append(_separtor);

            if (_random.Next(0, 100) == 7 && _duplicate.Any())
            {
                var str = _duplicate[_random.Next(0, _duplicate.Count - 1)];
                _stringBuilder.Append(str);

                if (_duplicate.Count > 100)
                    _duplicate.RemoveAt(_random.Next(0, _duplicate.Count - 1));

                return _stringBuilder.ToString();
            }

            PlusOne(_pattern.Count - 1);

            _stringBuilder.Append(_dict[_random.Next(0, _maxValue)]);
            _stringBuilder.Append(" ");

            for (int i = 0; i < _pattern.Count; i++)
            {
                _stringBuilder.Append(_dict[_pattern[i]]);

                if (i != _pattern.Count - 1)
                    _stringBuilder.Append(" ");
            }

            var result = _stringBuilder.ToString();

            if (_random.Next(0, 100) == 5)
            {
                _duplicate.Add(result.Split(_separtor)[1]);
            }

            return result;
        }

        private void PlusOne(int index)
        {
            if (_pattern[index] + 1 > _maxValue)
            {
                if (index == 0)
                {
                    _pattern.Add(0);
                    FillPattern();
                    return;
                }
                _pattern[index] = 0;
                PlusOne(index - 1);
            }
            else
                _pattern[index]++;
        }

        private void FillPattern()
        {
            for (int i = 0; i < _pattern.Count; i++)
            {
                _pattern[i] = _random.Next(0, _maxValue);
            }
        }

    }
}
