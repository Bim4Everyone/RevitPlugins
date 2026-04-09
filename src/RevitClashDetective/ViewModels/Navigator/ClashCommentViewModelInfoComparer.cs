using System;
using System.Collections.Generic;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashCommentViewModelInfoComparer : IEqualityComparer<ClashCommentViewModel> {
    public bool Equals(ClashCommentViewModel x, ClashCommentViewModel y) {
        if(ReferenceEquals(x, y)) {
            return true;
        }

        if(x is null) {
            return false;
        }

        if(y is null) {
            return false;
        }

        if(x.GetType() != y.GetType()) {
            return false;
        }

        return x.Author == y.Author
               && x.Body == y.Body
               && x.Date == y.Date;
    }

    public int GetHashCode(ClashCommentViewModel obj) {
        if(obj is null) {
            return 0;
        }

        int hashCode = -1521134295;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Author);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Body);
        hashCode = hashCode * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(obj.Date);
        return hashCode;
    }
}
