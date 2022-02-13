﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;
using dosymep.Revit;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class OpeningWidthConditionViewModel : BaseViewModel, IConditionViewModel {

        private double _minWidth;
        private double _maxWidth;

        public OpeningWidthConditionViewModel() {

        }

        public double MinWidth {
            get => _minWidth;
            set => this.RaiseAndSetIfChanged(ref _minWidth, value);
        }

        public double MaxWidth {
            get => _maxWidth;
            set => this.RaiseAndSetIfChanged(ref _maxWidth, value);
        }

        public bool Check(FamilyInstance elementInWall) {
            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));


            //Todo: после установки 2021 версии поправить
#if D2020 || R2020
            var elementWidth = elementInWall.GetParamValueOrDefault("ADSK_Размер_Ширина");
            if (elementWidth == null) {
                elementWidth = elementInWall.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            }
            if(elementWidth == null) {
                elementWidth = elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            }

            if(elementWidth == null)
                throw new ArgumentException(nameof(elementInWall), $"У элемента {elementInWall.Id} отсутствует параметр \"ADSK_Размер_Ширина\".");
            double openingWidth = UnitUtils.ConvertFromInternalUnits((double) elementWidth, DisplayUnitType.DUT_MILLIMETERS);
            
#else
            double openingWidth = UnitUtils.ConvertFromInternalUnits(
                (double) elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM), UnitTypeId.Millimeters);
#endif
            return MinWidth <= openingWidth && openingWidth < MaxWidth;
        }
    }
}
