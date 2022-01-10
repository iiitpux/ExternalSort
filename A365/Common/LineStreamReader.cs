using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A365.Common
{
    public class LineStreamReader
    {
        private string _fileName;
        public LineStreamReader(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            _fileName = fileName;
        }

        public void Init()
        {
            _streamReader = new StreamReader(_fileName);
            SetValueAsync();
        }

        public bool IsActive { private set; get; } = true;

        public string Value { get; set; }

        private StreamReader _streamReader { get; set; }


        private void SetValueAsync()
        {
            Value = _streamReader.ReadLine();
            if (Value == null)
            {
                IsActive = false;
                _streamReader.Dispose();
            }
        }

        public string PopAsync()
        {
            var val = Value;

            SetValueAsync();

            return val;
        }
    }
}
