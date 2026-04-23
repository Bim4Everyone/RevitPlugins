using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

/// <summary>
/// Пары коллизий из одной пары отчетов для мержа
/// </summary>
internal class ClashesMergePairGroups {
    private static readonly ClashCommentContentComparer _commentsComparer = new();

    /// <summary>
    /// Создает экземпляр класса, разбивая пары коллизий по логическим группам
    /// </summary>
    /// <param name="clashPairs">Пары коллизий из одной пары отчетов для мержа</param>
    public ClashesMergePairGroups(ICollection<ClashMergePairViewModel> clashPairs) {
        if(clashPairs == null) {
            throw new ArgumentNullException(nameof(clashPairs));
        }

        Initialize(clashPairs);
    }

    /// <summary>
    /// Пары коллизий, в которых импортируемая и существующая коллизии - одинаковые,
    /// или свойства импортируемой коллизии имеют дефолтные значения 
    /// </summary>
    public ICollection<ClashMergePairViewModel> Unchanged { get; } = new List<ClashMergePairViewModel>();

    /// <summary>
    /// Пары коллизий, в которых присутствует конфликт значений свойств.
    /// Одно и то же свойство было изменено относительно значений по умолчанию и у импортируемой и у существующей коллизии.
    /// </summary>
    public ICollection<ClashMergePairViewModel> Conflicted { get; } = new List<ClashMergePairViewModel>();

    /// <summary>
    /// Пары коллизий, в которых и у существующей и у импортируемой коллизии изменены свойства относительно значений по умолчанию,
    /// но нет конфликта свойств. 
    /// </summary>
    public ICollection<ClashMergePairViewModel> NonConflicted { get; } = new List<ClashMergePairViewModel>();

    private void Initialize(ICollection<ClashMergePairViewModel> clashPairs) {
        foreach(var pair in clashPairs) {
            if(IsDefault(pair.Importing)
               || IsEqual(pair.Existing, pair.Importing)) {
                pair.ExistingClashSelected = true;
                Unchanged.Add(pair);
                continue;
            }

            if(IsDefault(pair.Existing)
               && !IsDefault(pair.Importing)) {
                pair.ImportingClashSelected = true;
                NonConflicted.Add(pair);
                continue;
            }

            if(HasNoConflicts(pair.Existing, pair.Importing)) {
                pair.ExistingClashSelected = true;
                if(NameIsChanged(pair.Existing, pair.Importing)) {
                    pair.ImportingNameSelected = true;
                }
                if(StatusIsChanged(pair.Existing, pair.Importing)) {
                    pair.ImportingStatusSelected = true;
                }

                // комментарии по умолчанию объединяются
                pair.ImportingCommentsSelected = true;
                NonConflicted.Add(pair);
                continue;
            }

            pair.ExistingClashSelected = true;
            Conflicted.Add(pair);
        }
    }

    private bool IsDefault(ClashViewModel clash) {
        if(!HasDefaultName(clash)) {
            return false;
        }

        if(!HasDefaultStatus(clash)) {
            return false;
        }

        if(!HasDefaultComments(clash)) {
            return false;
        }

        return true;
    }

    private bool NameIsChanged(ClashViewModel old, ClashViewModel @new) {
        return HasDefaultName(old) && !HasDefaultName(@new);
    }

    private bool StatusIsChanged(ClashViewModel old, ClashViewModel @new) {
        return HasDefaultStatus(old) && !HasDefaultStatus(@new);
    }

    private bool HasNoConflicts(ClashViewModel left, ClashViewModel right) {
        return ((HasDefaultName(left) ^ HasDefaultName(right)) || (left.ClashName == right.ClashName))
               && ((HasDefaultStatus(left) ^ HasDefaultStatus(right)) || (left.ClashStatus == right.ClashStatus));
    }

    private bool IsEqual(ClashViewModel left, ClashViewModel right) {
        if(left.ClashName != right.ClashName) {
            return false;
        }

        if(left.ClashStatus != right.ClashStatus) {
            return false;
        }

        if((left.Comments.Count != right.Comments.Count)
           || (left.Comments.Intersect(right.Comments, _commentsComparer).Count() != left.Comments.Count)) {
            return false;
        }

        return true;
    }

    private bool HasDefaultName(ClashViewModel clash) {
        const string namePattern = $@"^{ClashDetector.DefaultNamePrefix}\d+$";
        return Regex.IsMatch(clash.ClashName, namePattern);
    }

    private bool HasDefaultStatus(ClashViewModel clash) {
        return clash.ClashStatus == ClashStatus.Active;
    }

    private bool HasDefaultComments(ClashViewModel clash) {
        return clash.Comments.Count == 0;
    }
}
