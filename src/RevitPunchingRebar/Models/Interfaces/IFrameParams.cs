using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitPunchingRebar.Models.Interfaces;
internal interface IFrameParams {
    string FrameFamilyName { get; set; }
    string FrameFamilyTypeName { get; set; }

    double SlabThickness { get; set; }
    Slab HostSlab { get; set; }
    double RebarCoverTop { get; set; }
    double RebarCoverBottom { get; set; }
    double PlateRebarDiameter { get; set; }
    
    int StirrupRebarClass { get; set; }
    double StirrupRebarDiameter { get; set; }
    double StirrupStep { get; set; }

    double FrameWidth { get; set; }

    /// <summary>
    /// Находит длину каркаса (без учета "хвостиков")
    /// </summary>
    /// <returns></returns>
    double GetFrameLength();

    /// <summary>
    /// Находит высоту каркаса (расстояние между ц.т. 
    /// продольных стержней)
    /// </summary>
    /// <returns></returns>
    double GetFrameHeight(double longRebarDiameter);

    /// <summary>
    /// Находит расстояние от грани колонны до первого хомута каркаса
    /// </summary>
    /// <returns></returns>
    double GetAfterPylonDistance();

    /// <summary>
    /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
    /// </summary>
    /// <returns></returns>
    double GetPunchingZone();
      
}
