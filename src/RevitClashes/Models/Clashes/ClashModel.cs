using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.Models.Clashes;
internal class ClashModel : IEquatable<ClashModel> {
    private RevitRepository _revitRepository;


    [JsonConstructor]
    public ClashModel() { }

    public ClashModel(RevitRepository revitRepository, Element mainElement, Element otherElement)
        : this(revitRepository, mainElement, Transform.Identity, otherElement, Transform.Identity) {
    }

    public ClashModel(
        RevitRepository revitRepository,
        Element mainElement,
        Transform mainElementTransform,
        Element otherElement,
        Transform otherElementTransform) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        if(mainElement is null) { throw new ArgumentNullException(nameof(mainElement)); }
        if(mainElementTransform is null) { throw new ArgumentNullException(nameof(mainElementTransform)); }
        if(otherElement is null) { throw new ArgumentNullException(nameof(otherElement)); }
        if(otherElementTransform is null) { throw new ArgumentNullException(nameof(otherElementTransform)); }

        MainElement = new ElementModel(mainElement, mainElementTransform);
        OtherElement = new ElementModel(otherElement, otherElementTransform);
    }


    public string Name { get; set; }
    public ClashStatus ClashStatus { get; set; }
    public ElementModel MainElement { get; set; }
    public ElementModel OtherElement { get; set; }


    public ClashModel SetRevitRepository(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        return this;
    }

    /// <summary>
    /// Проверяет, что хотя бы 1 элемент коллизии находится в одном из заданных файлов
    /// </summary>
    /// <param name="documentNames">Коллекция названий файлов для проверки</param>
    /// <returns>True, если хотя бы 1 элемент коллизии находится в одном из заданных файлов, иначе False</returns>
    public bool IsValid(ICollection<string> documentNames) {
        var clashElements = new[] { MainElement, OtherElement };
        return clashElements.Where(el => el != null)
            .Where(e => documentNames.Any(d => d.Contains(e?.DocumentName)))
            .Select(e => _revitRepository.GetElement(e.DocumentName, e.Id))
            .Where(e => e != null)
            .Where(e => e.GetTypeId().IsNotNull())
            .Count() > 0;
    }

    public override bool Equals(object obj) {
        return Equals(obj as ClashModel);
    }

    public override int GetHashCode() {
        int hashCode = 2096115351;
        hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(MainElement);
        hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(OtherElement);
        return hashCode;
    }

    public bool Equals(ClashModel other) {
        return other is not null && (ReferenceEquals(this, other) || (Equals(MainElement, other.MainElement)
            && Equals(OtherElement, other.OtherElement))
            || (Equals(MainElement, other.OtherElement)
            && Equals(OtherElement, other.MainElement)));
    }

    /// <summary>
    /// Возвращает данные о коллизии. Необходимо, чтобы <see cref="_revitRepository"/> не был null.
    /// Назначить это поле можно через метод <see cref="SetRevitRepository(RevitRepository)"/>
    /// </summary>
    /// <returns>Объект с данными о коллизии</returns>
    /// <exception cref="InvalidOperationException">Исключение, если <see cref="_revitRepository"/> null</exception>
    public ClashData GetClashData() {
        if(_revitRepository is null) {
            throw new InvalidOperationException(
                "Для получения данных о коллизии необходимо назначить репозиторий активного документа Ревита");
        }

        try {
            var mainSolids = GetMainSolids();
            var otherSolids = GetOtherSolids();

            double intersectionVolume = GetIntersectionVolume(mainSolids, otherSolids);

            double mainSolidsV = mainSolids.Sum(s => s.Volume);
            double otherSolidsV = otherSolids.Sum(s => s.Volume);
            return new ClashData(mainSolidsV, otherSolidsV, intersectionVolume);
        } catch(ArgumentNullException) {
            return new ClashData();
        } catch(NullReferenceException) {
            return new ClashData();
        } catch(ArgumentException) {
            return new ClashData();
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return new ClashData();
        }
    }


    private double GetIntersectionVolume(IEnumerable<Solid> first, IEnumerable<Solid> second) {
        double intersection = 0;
        try {
            foreach(var solid1 in first) {
                foreach(var solid2 in second) {
                    intersection += BooleanOperationsUtils.ExecuteBooleanOperation(
                        solid1,
                        solid2,
                        BooleanOperationsType.Intersect).Volume;
                }
            }
        } catch(NullReferenceException) {
            intersection = 0;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            intersection = 0;
        }
        return intersection;
    }

    private IList<Solid> GetMainSolids() {
        return MainElement.GetSolids(_revitRepository.DocInfos);
    }

    private IList<Solid> GetOtherSolids() {
        var transform = MainElement.GetTransform()
                .GetTransitionMatrix(OtherElement.GetTransform());
        return OtherElement.GetSolids(_revitRepository.DocInfos)
                .Select(s => SolidUtils.CreateTransformed(s, transform))
                .ToList();
    }
}

internal enum ClashStatus {
    [Display(Name = "Активно")]
    Active,
    [Display(Name = "Проанализировано")]
    Analized,
    [Display(Name = "Исправлено")]
    Solved,
    [Display(Name = "Нет пересечения")]
    Imaginary
}
