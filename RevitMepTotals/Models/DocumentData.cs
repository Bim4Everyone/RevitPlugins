using System;
using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class DocumentData : IDocumentData {
        private readonly List<IDuctData> _ductData = new List<IDuctData>();
        private readonly List<IPipeData> _pipeData = new List<IPipeData>();
        private readonly List<IPipeInsulationData> _pipeInsulationData = new List<IPipeInsulationData>();


        public DocumentData(string title) {
            if(string.IsNullOrWhiteSpace(title)) { throw new ArgumentException(nameof(title)); }

            Title = title;
        }


        public string Title { get; }

        public ICollection<IDuctData> Ducts => _ductData;

        public ICollection<IPipeData> Pipes => _pipeData;

        public ICollection<IPipeInsulationData> PipeInsulations => _pipeInsulationData;


        public void AddDuctData(IDuctData data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            _ductData.Add(data);
        }

        public void AddDuctData(ICollection<IDuctData> data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            foreach(IDuctData ductData in data) {
                AddDuctData(ductData);
            }
        }

        public void AddPipeData(IPipeData data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            _pipeData.Add(data);
        }

        public void AddPipeData(ICollection<IPipeData> data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            foreach(IPipeData ductData in data) {
                AddPipeData(ductData);
            }
        }

        public void AddPipeInsulationData(IPipeInsulationData data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            _pipeInsulationData.Add(data);
        }

        public void AddPipeInsulationData(ICollection<IPipeInsulationData> data) {
            if(data == null) { throw new ArgumentNullException(nameof(data)); }

            foreach(IPipeInsulationData ductInsulationData in data) {
                AddPipeInsulationData(ductInsulationData);
            }
        }
    }
}
