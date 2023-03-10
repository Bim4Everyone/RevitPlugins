using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для <see cref="Autodesk.Revit.DB.EdgeArrayArray">EdgeArrayArray</see>
    /// </summary>
    internal static class EdgeArrayArrayExtension {
        /// <summary>
        /// Возвращает самый длинный <see cref="Autodesk.Revit.DB.EdgeArray">EdgeArray</see>
        /// </summary>
        /// <param name="edgeArrayArray"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Исключение, если <see cref="Autodesk.Revit.DB.EdgeArrayArray">массив массивов ребер</see> пустой</exception>
        public static EdgeArray GetLongestEdgeArray(this EdgeArrayArray edgeArrayArray) {
            var size = edgeArrayArray.Size;
            if(size == 0) {
                throw new ArgumentException($"{nameof(edgeArrayArray)} is empty.");
            } else {
                EdgeArray longestEdgeArray = edgeArrayArray.get_Item(0);
                double longestEdgeArrayLength = longestEdgeArray.GetLength();

                foreach(EdgeArray edgeArrayCurrent in edgeArrayArray) {
                    double lengthCurrent = edgeArrayCurrent.GetLength();
                    if(lengthCurrent > longestEdgeArrayLength) {
                        longestEdgeArrayLength = lengthCurrent;
                        longestEdgeArray = edgeArrayCurrent;
                    }
                }
                return longestEdgeArray;
            }
        }
    }
}
