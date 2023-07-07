using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitIsolateByParameter.Models {
    internal class ParamValue {
        public ParamValue(string value) {
            Value = value;
            UserValue = value;
        }

        /// <summary>
        /// Реальное значения параметра для фильтрации.
        /// </summary>
        public string Value { get; set; }

        private string _userValue;

        /// <summary>
        /// Значения параметра для отображения в пользовательском окне.
        /// </summary>
        public string UserValue { 
            get { return _userValue; }
            set { 
                if(value == null) _userValue = "<Параметр не заполнен>";
                else _userValue = value; 
            }
        }
    }
}
