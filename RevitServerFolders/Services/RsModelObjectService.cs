﻿using System.Collections.Generic;
using System.Threading.Tasks;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services {
    internal sealed class RsModelObjectService : IModelObjectService {
        public Task<ModelObject> SelectModelObjectDialog() {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ModelObject>> SelectModelObjectsDialog() {
            throw new System.NotImplementedException();
        }
    }
}
