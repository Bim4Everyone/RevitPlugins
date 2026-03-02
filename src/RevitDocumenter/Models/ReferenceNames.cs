using System.Collections.Generic;

namespace RevitDocumenter.Models;
internal class ReferenceNames {

    public ReferenceNames(List<string> verticalRefName, List<string> horizontalRefNames) {
        VerticalRefNames = verticalRefName;
        HorizontalRefNames = horizontalRefNames;
    }

    public List<string> VerticalRefNames;
    public List<string> HorizontalRefNames;
}
