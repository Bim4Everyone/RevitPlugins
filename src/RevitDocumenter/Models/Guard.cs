using System;
using System.Collections;

namespace RevitDocumenter.Models;
internal class Guard {
    internal void ThrowIfNull(params object[] objects) {
        if(objects == null) {
            throw new ArgumentNullException(nameof(objects));
        }
    }

    internal void ThrowIfNullOrEmpty(params object[] objects) {
        ThrowIfNull(objects);
        for(int i = 0; i < objects.Length; i++) {
            if(objects[i] == null) {
                throw new ArgumentNullException($"Parameter at index {i} is null");
            }
            if(objects[i] is IList list && list.Count == 0) {
                throw new ArgumentException($"Parameter at index {i} is an empty collection");
            }
        }
    }
}
