using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.Services;
internal class TagCreationService {
    private readonly PylonView _pylonView;

    private readonly string _annotationTagTopTextParamName = "Текст верх";
    private readonly string _annotationTagBottomTextParamName = "Текст низ";
    private readonly string _annotationTagLengthParamName = "Ширина полки";

    public TagCreationService(PylonView pylonView) {
        _pylonView = pylonView;
    }

    public IndependentTag CreateRebarTag(TagOption tagOption, Element element) {
        if(tagOption is null) {
            throw new ArgumentNullException(nameof(tagOption));
        }
        if(tagOption.TagSymbol is null) {
            throw new ArgumentNullException(nameof(tagOption.TagSymbol));
        }
        if(tagOption.BodyPoint is null) {
            throw new ArgumentNullException(nameof(tagOption.BodyPoint));
        }

        var view = _pylonView.ViewElement;
        var doc = view.Document;
        var annotationInstance = IndependentTag.Create(doc, tagOption.TagSymbol.Id, view.Id, new Reference(element),
                                                       true, TagOrientation.Horizontal, tagOption.BodyPoint);
        annotationInstance.TagHeadPosition = tagOption.BodyPoint;
        return annotationInstance;
    }

    public IndependentTag CreateRebarTag(TagOption tagOption, List<Element> elements) {
        if(tagOption is null) {
            throw new ArgumentNullException(nameof(tagOption));
        }
        if(tagOption.TagSymbol is null) {
            throw new ArgumentNullException(nameof(tagOption.TagSymbol));
        }
        if(tagOption.BodyPoint is null) {
            throw new ArgumentNullException(nameof(tagOption.BodyPoint));
        }

        if(elements is null) {
            throw new ArgumentNullException(nameof(elements));
        }
        if(elements.Count == 0) {
            throw new ArgumentException(nameof(elements.Count));
        }

        var view = _pylonView.ViewElement;
        var doc = view.Document;
        var refs = elements.Select(e => new Reference(e)).ToList();
        var keyRef = refs.FirstOrDefault();

        var annotationInstance = IndependentTag.Create(doc, tagOption.TagSymbol.Id, view.Id, keyRef,
                                                       true, TagOrientation.Horizontal, tagOption.BodyPoint);
        annotationInstance.TagHeadPosition = tagOption.BodyPoint;

#if REVIT_2022_OR_GREATER
        if(refs.Count > 1) {
            refs.RemoveAt(0);
        }
        annotationInstance.AddReferences(refs);
#endif
        return annotationInstance;
    }

    public void CreateUniversalTag(TagOption tagOption, Element element, XYZ leaderPoint = null) {
        if(tagOption is null) {
            throw new ArgumentNullException(nameof(tagOption));
        }
        if(tagOption.TagSymbol is null) {
            throw new ArgumentNullException(nameof(tagOption.TagSymbol));
        }
        if(tagOption.BodyPoint is null) {
            throw new ArgumentNullException(nameof(tagOption.BodyPoint));
        }
        if(tagOption.TagLength == 0) {
            throw new ArgumentException(nameof(tagOption.TagLength));
        }
        if(element is null) {
            throw new ArgumentException(nameof(element));
        }

        var annotationInstance = CreateAnnotationTag(tagOption);
        // Добавляем и устанавливаем точку привязки выноски
        annotationInstance.addLeader();
        var leader = annotationInstance.GetLeaders().FirstOrDefault();
        if(leader != null) {
            if(leaderPoint is null) {
                var loc = element.Location as LocationPoint;
                leader.End = loc.Point; 
            } else {
                leader.End = leaderPoint;
            }
        }
    }

    public void CreateUniversalTag(TagOption tagOption, XYZ leaderPoint) {
        if(tagOption is null) {
            throw new ArgumentNullException(nameof(tagOption));
        }
        if(tagOption.TagSymbol is null) {
            throw new ArgumentNullException(nameof(tagOption.TagSymbol));
        }
        if(tagOption.BodyPoint is null) {
            throw new ArgumentNullException(nameof(tagOption.BodyPoint));
        }
        if(tagOption.TagLength == 0) {
            throw new ArgumentException(nameof(tagOption.TagLength));
        }
        if(leaderPoint is null) {
            throw new ArgumentNullException(nameof(leaderPoint));
        }

        var annotationInstance = CreateAnnotationTag(tagOption);
        // Добавляем и устанавливаем точку привязки выноски
        annotationInstance.addLeader();
        var leader = annotationInstance.GetLeaders().FirstOrDefault();
        leader.End = leaderPoint;
    }

    public AnnotationSymbol CreateAnnotationTag(TagOption tagOption) {
        if(tagOption is null) {
            throw new ArgumentNullException(nameof(tagOption));
        }
        if(tagOption.TagSymbol is null) {
            throw new ArgumentNullException(nameof(tagOption.TagSymbol));
        }
        if(tagOption.BodyPoint is null) {
            throw new ArgumentNullException(nameof(tagOption.BodyPoint));
        }
        if(tagOption.TagLength == 0) {
            throw new ArgumentException(nameof(tagOption.TagLength));
        }

        var view = _pylonView.ViewElement;
        var doc = view.Document;
        // Создаем экземпляр типовой аннотации для указания ГОСТа
        var annotationInstance = 
            doc.Create.NewFamilyInstance(tagOption.BodyPoint, tagOption.TagSymbol, view) as AnnotationSymbol;

        // Устанавливаем значение верхнего текста у выноски
        if(tagOption.TopText != null) {
            annotationInstance.SetParamValue(_annotationTagTopTextParamName, tagOption.TopText);
        }
        // Устанавливаем значение нижнего текста у выноски
        if(tagOption.BottomText != null) {
            annotationInstance.SetParamValue(_annotationTagBottomTextParamName, tagOption.BottomText);
        }
        // Устанавливаем значение длины полки под текстом, чтобы текст влез
        annotationInstance.SetParamValue(_annotationTagLengthParamName, tagOption.TagLength);
        return annotationInstance;
    }
}
