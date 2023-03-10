using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    /// <summary>
    /// Методы расширения для <see cref="Autodesk.Revit.DB.EdgeArray">EdgeArray</see>
    /// </summary>
    internal static class EdgeArrayExtension {
        /// <summary>
        /// Возвращает точную суммарную длину <see cref="Autodesk.Revit.DB.EdgeArray">массива ребер</see> в единицах Revit
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static double GetLength(this EdgeArray edgeArray) {
            double length = 0;
            foreach(Edge edge in edgeArray) {
                length += edge.AsCurve().Length;
            }
            return length;
        }

        /// <summary>
        /// Преобразует <see cref="EdgeArray">EdgeArray</see> в <see cref="Autodesk.Revit.DB.CurveLoop">CurveLoop</see>
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static CurveLoop AsCurveLoop(this EdgeArray edgeArray) {
            CurveLoop loop = new CurveLoop();
            for(int i = 0; i < edgeArray.Size; i++) {
                loop.Append(edgeArray.get_Item(i).AsCurve());
            }
            return loop;
        }
    }
}
