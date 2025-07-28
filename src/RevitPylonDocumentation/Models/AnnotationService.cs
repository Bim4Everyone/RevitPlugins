using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models;
internal class AnnotationService {
    private readonly PylonView _pylonView;

    private readonly string _annotationTagTopTextParamName = "Текст верх";
    private readonly string _annotationTagBottomTextParamName = "Текст низ";
    private readonly string _annotationTagLengthParamName = "Ширина полки";


    public AnnotationService(PylonView pylonView) {
        _pylonView = pylonView;
    }


    public IndependentTag CreateRebarTag(XYZ bodyPoint, FamilySymbol tagSymbol, Element element) {
        var view = _pylonView.ViewElement;
        var doc = view.Document;
        var annotationInstance = IndependentTag.Create(doc, tagSymbol.Id, view.Id, new Reference(element),
                          true, TagOrientation.Horizontal, bodyPoint);
        annotationInstance.TagHeadPosition = bodyPoint;
        return annotationInstance;
    }

    public IndependentTag CreateRebarTag(XYZ bodyPoint, FamilySymbol tagSymbol, List<Element> elements) {
        if(elements is null || elements.Count == 0) {
            return null;
        }
        var view = _pylonView.ViewElement;
        var doc = view.Document;
        var refs = elements.Select(e => new Reference(e)).ToList();
        var keyRef = refs.FirstOrDefault();

        var annotationInstance = IndependentTag.Create(doc, tagSymbol.Id, view.Id, keyRef,
                          true, TagOrientation.Horizontal, bodyPoint);
        annotationInstance.TagHeadPosition = bodyPoint;

#if REVIT_2022_OR_GREATER
        if(refs.Count > 1) {
            refs.RemoveAt(0);
        }
        annotationInstance.AddReferences(refs);
#endif
        return annotationInstance;
    }


    public void CreateUniversalTag(XYZ bodyPoint, FamilySymbol annotationSymbol, Element element, 
                                   string topText = null, string bottomText = null) {
        var annotationInstance = CreateAnnotationSymbol(bodyPoint, annotationSymbol, topText, bottomText);

        // Добавляем и устанавливаем точку привязки выноски
        annotationInstance.addLeader();
        Leader leader = annotationInstance.GetLeaders().FirstOrDefault();
        if(leader != null) {
            var loc = element.Location as LocationPoint;
            leader.End = loc.Point; // Точка на элементе
        }
    }

    public void CreateUniversalTag(XYZ bodyPoint, FamilySymbol annotationSymbol, XYZ leaderPoint,
                                   string topText = null, string bottomText = null) {
        var annotationInstance = CreateAnnotationSymbol(bodyPoint, annotationSymbol, topText, bottomText);

        // Добавляем и устанавливаем точку привязки выноски
        annotationInstance.addLeader();
        Leader leader = annotationInstance.GetLeaders().FirstOrDefault();
        if(leader != null) {
            leader.End = leaderPoint; // Точка на элементе
        }
    }

    private AnnotationSymbol CreateAnnotationSymbol(XYZ bodyPoint, FamilySymbol annotationSymbol,
                                                    string topText = null, string bottomText = null) {
        var view = _pylonView.ViewElement;
        var doc = view.Document;
        // Создаем экземпляр типовой аннотации для указания ГОСТа
        var annotationInstance = doc.Create.NewFamilyInstance(bodyPoint, annotationSymbol, view) as AnnotationSymbol;

        // Устанавливаем значение верхнего текста у выноски
        if(topText != null) {
            annotationInstance.SetParamValue(_annotationTagTopTextParamName, topText);
        }
        // Устанавливаем значение нижнего текста у выноски
        if(bottomText != null) {
            annotationInstance.SetParamValue(_annotationTagBottomTextParamName, bottomText);
        }
        // Устанавливаем значение длины полки под текстом, чтобы текст влез
        annotationInstance.SetParamValue(_annotationTagLengthParamName, UnitUtilsHelper.ConvertToInternalValue(40));
        return annotationInstance;
    }
}
