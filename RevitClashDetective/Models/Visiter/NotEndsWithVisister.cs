﻿using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class NotEndsWithVisister : IVisiter {
        public FilterRule Create(ElementId paramId, string value) {
            return ParameterFilterRuleFactory.CreateNotEndsWithRule(paramId, value, false);
        }

        public FilterRule Create(ElementId paramId, int value) {
            throw new NotImplementedException();
        }

        public FilterRule Create(ElementId paramId, double value) {
            throw new NotImplementedException();
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
