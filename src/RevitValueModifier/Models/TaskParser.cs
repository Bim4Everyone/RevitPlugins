using System;
using System.Text.RegularExpressions;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

namespace RevitValueModifier.Models {
    internal class TaskParser {

        internal void ParseTask(string taskForWrite) {



            TaskDialog.Show("taskForWrite1", taskForWrite);

            // заранее реализовать проверку, что символы { } парны
            // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2

            Regex regex = new Regex(@"{([^\}]+)}");
            MatchCollection matches = regex.Matches(taskForWrite);
            if(matches.Count > 0) {
                foreach(Match match in matches)
                    TaskDialog.Show("Value", match.Value);
            } else {
                Console.WriteLine("Совпадений не найдено");
            }
        }
    }
}
