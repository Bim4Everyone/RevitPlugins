using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using Vanara.PInvoke;

namespace RevitBatchPrint.Models.Printing {
    internal class PrinterSettings : IDisposable {
        private readonly string _printerName;
        private readonly WinSpool.SafeHPRINTER _shPrinter;

        public PrinterSettings(string printerName) {
            _printerName = printerName;
            _shPrinter = OpenPrinter2(printerName);
        }

        public string PrinterName => _printerName;

        public bool HasFormatName(string formatName) {
            return GetFormatNames().Contains(formatName);
        }

        public IEnumerable<string> GetFormatNames() {
            return WinSpool.EnumForms<WinSpool.FORM_INFO_2>(_shPrinter).Select(item => item.pName);
        }

        public void AddFormat(string formatName, Size formatSize) {
            AddFormat(formatName, formatSize, new Rectangle(Point.Empty, formatSize));
        }

        public void AddFormat(string formatName, Size formatSize, Rectangle visibleArea) {
            const int coeff = 1000;
            var options = new WinSpool.FORM_INFO_2() {
                pName = formatName,
                pKeyword = formatName,
                Flags = WinSpool.FormFlags.FORM_USER,
                StringType = WinSpool.FormStringType.STRING_NONE,
                Size = new SIZE(formatSize.Width * coeff, formatSize.Height * coeff),
                ImageableArea = new RECT(visibleArea.Left * coeff, visibleArea.Top * coeff, visibleArea.Right * coeff,
                    visibleArea.Bottom * coeff)
            };

            ThrowWin32Exception(WinSpool.AddForm(_shPrinter, in options));
        }

        public void RemoveFormat(string formatName) {
            ThrowWin32Exception(WinSpool.DeleteForm(_shPrinter, formatName));
        }

        private WinSpool.SafeHPRINTER OpenPrinter2(string printerName) {
            var pDefault = new WinSpool.PRINTER_DEFAULTS();
            var pOptions = WinSpool.PRINTER_OPTIONS.Default;
            ThrowWin32Exception(WinSpool.OpenPrinter2(printerName, out WinSpool.SafeHPRINTER phPrinter, pDefault,
                in pOptions));

            return phPrinter;
        }

        private void ClosePrinter() {
            WinSpool.ClosePrinter(_shPrinter);
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
                    _shPrinter.Close();
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