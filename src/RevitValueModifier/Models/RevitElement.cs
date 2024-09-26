using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitElement : INotifyPropertyChanged {
        private string _paramValue = string.Empty;

        public RevitElement(Element element) {
            Elem = element;
            Parameters = Elem.Parameters.Cast<Parameter>().ToList();
        }


        public Element Elem { get; }
        public List<Parameter> Parameters { get; set; }

        public string ParamValue {
            get { return _paramValue; }
            set {
                _paramValue = value;
                OnPropertyChanged("ParamValue");
            }
        }


        public void SetParamValue(string paramValueMask) {
            // заранее реализовать проверку, что символы { } парны
            // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
            ParamValue = paramValueMask;
            Regex regex = new Regex(@"{([^\}]+)}");
            MatchCollection matches = regex.Matches(paramValueMask);
            //if(matches.Count == 0) {
            //    Console.WriteLine("Совпадений не найдено");
            //    return;
            //}

            Regex regexForParam;
            foreach(Match match in matches) {
                string paramName = match.Value.Replace("{", "").Replace("}", "");
                Parameter param = Parameters
                    .FirstOrDefault(parameter => parameter.Definition.Name == paramName);
                if(param == null) { return; }

                regexForParam = new Regex(paramName);
                var val = GetParamValue(param);
                ParamValue = regexForParam.Replace(ParamValue, val, 1);
            }
        }




        public string GetParamValue(Parameter parameter) {
            if(!parameter.HasValue) {
                return string.Empty;
            }

            string value;
            //var def = parameter.Definition;

            switch(parameter.StorageType) {
                case StorageType.Double:
                    value = parameter.AsValueString();
                    break;
                case StorageType.ElementId:
                    ElementId id = parameter.AsElementId();

#if REVIT_2024_OR_GREATER
                    if(id.Value >= 0) {
                        value = Elem.Document.GetElement(id).Name;
                    } else {
                        value = id.Value.ToString();
                    }
#else
                    if(id.IntegerValue >= 0) {
                        value = Elem.Document.GetElement(id).Name;
                    } else {
                        value = id.IntegerValue.ToString();
                    }
#endif
                    break;
                case StorageType.Integer:
                    if(SpecTypeId.Boolean.YesNo == parameter.Definition.GetDataType()) {
                        if(parameter.AsInteger() == 0) {
                            value = "False";
                        } else {
                            value = "True";
                        }
                    } else {
                        value = parameter.AsInteger().ToString();
                    }
                    break;
                case StorageType.String:
                    value = parameter.AsString();
                    break;
                default:
                    value = "Значение неизвестно";
                    break;
            }

            return value;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
