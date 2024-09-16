using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class NameGroupFactory {
        private readonly SpecConfiguration _config;
        private readonly Document _document;
        private readonly VisElementsCalculator _calculator;
        public NameGroupFactory(
            SpecConfiguration configuration,
            Document document,
            VisElementsCalculator calculator
            ) {
            _document = document;
            _config = configuration;
            _calculator = calculator;          
        }

    }
}
