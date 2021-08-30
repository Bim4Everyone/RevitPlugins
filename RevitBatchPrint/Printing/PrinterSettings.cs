using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RevitBatchPrint.Printing {
    internal class PrinterSettings : IDisposable {
        private readonly string _printerName;

        public PrinterSettings(string printerName) {
            _printerName = printerName;
        }

        public void AddFormat(string formatName, Size formatSize) {
            AddFormat(formatName, formatSize, new Rectangle(Point.Empty, formatSize));
        }

        public void AddFormat(string formatName, Size formatSize, Rectangle visibleArea) {
            throw new NotImplementedException();
        }

        public void RemoveForm(string formatName) {
            throw new NotImplementedException();
        }

        private void ClosePrinter() {
            throw new NotImplementedException();
        }

        #region IDisposable

        private bool _isDisposed;

        ~PrinterSettings() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if(_isDisposed) {
                ClosePrinter();
                if(disposing) {

                }

                _isDisposed = true;
            }
        }

        #endregion

        private void ThrowWin32Exception(bool result) {
            if(!result) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
