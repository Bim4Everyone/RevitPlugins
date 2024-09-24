using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitElemUtils {
        internal RevitElemUtils(List<Element> elements) {
            Elements = elements;
        }

        public List<Element> Elements { get; }
        public List<RevitElem> RevitElems { get; set; } = new List<RevitElem>();


        internal void GetRevitElems() {
            if(Elements is null || Elements.Count == 0) {
                throw new Exception("Элементы не найдены!");
            }

            foreach(Element element in Elements) {
                RevitElems.Add(new RevitElem(element));
            }
        }

        internal void GetElemParameters() {
            foreach(RevitElem revitElem in RevitElems) {
                revitElem.GetElementParameters();
            };
        }

        //public List<RevitElem> GetRevitElems(List<Element> elements, List<ElementId> parameterIds) {
        //    return elements
        //        .Select(element => GetRevitElem(element, parameterIds))
        //        .ToList();
        //}



        //public RevitElem GetRevitElem(Element element, List<ElementId> parameterIds) {
        //    List<ParamValuePair> paramValuePairList = element.Parameters
        //        .Cast<Parameter>()
        //        .Where(p => parameterIds.Contains(p.Id))
        //        .Select(p => GetParamValuePair(p))
        //        .ToList();
        //    return new RevitElem(element, paramValuePairList);
        //}






        //public void GetParamValuePairs(
        //    List<RevitElem> revitElems,
        //    List<ElementId> parameterIds,
        //    RevitParameterHelper revitParameterHelper) {

        //    List<ParamValuePair> paramValuePairList = revitElems
        //        .Select(revitElem => revitElem.Parameters)
        //        .Where(p => parameterIds.Contains(p.Id))
        //        .Select(p => GetParamValuePairForRevitElem(p))
        //        .ToList();



        //    var ttt = revitElems
        //        .Select(revitElem => revitElem.GetParamValuePairs(revitParameterHelper));




        //    ParamValuePair paramValuePair = revitParameterHelper.GetParamValuePair(parameter);


        //    revitElem = paramValuePair;
        //}



        //private void GetParamValuePairForRevitElem(
        //    RevitElem revitElem,
        //    List<ElementId> parameterIds,
        //    RevitParameterHelper revitParameterHelper) {

        //    var f = revitElem.Parameters
        //        .Where(p => parameterIds.Contains(p.Id))
        //        .Select(p => revitElem.AddParamValuePair(p));




        //    List<ParamValuePair> paramValuePairList = revitElems
        //        .Select(revitElem => revitElem.Parameters)
        //        .Where(p => parameterIds.Contains(p.Id))
        //        .Select(p => GetParamValuePair(p))
        //        .ToList();


        //}
    }
}
