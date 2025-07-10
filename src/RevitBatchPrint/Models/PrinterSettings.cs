using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using Vanara.PInvoke;

namespace RevitBatchPrint.Models {
    internal class PrinterSettings : IPrinterSettings {
        // Коэффициент перевода в микроны.
        // Микроны используются для назначения размера формата страницы.
        private const int _micronsPerMm = 1000;

        private readonly string _printerName;
        private IReadOnlyCollection<string> _printerFormats;

        public PrinterSettings(string printerName) {
            _printerName = printerName;
        }

        public IPrinterSettings Load() {
            _printerFormats = SafeExecute(handle => WinSpool.EnumForms<WinSpool.FORM_INFO_2>(handle)
                .Select(item => item.pName)
                .ToArray());

            return this;
        }

        public bool HasFormat(string formatName) {
            if(string.IsNullOrWhiteSpace(formatName)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(formatName));
            }

            return _printerFormats.Contains(formatName);
        }

        public IEnumerable<string> EnumFormatNames() {
            return _printerFormats;
        }

        public void AddFormat(string formatName, Size formatSize) {
            if(string.IsNullOrWhiteSpace(formatName)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(formatName));
            }

            if(formatSize.Width <= 0 || formatSize.Height <= 0) {
                throw new ArgumentException("Size must be positive.", nameof(formatSize));
            }

            AddFormat(formatName, formatSize, new Rectangle(Point.Empty, formatSize));
            Load();
        }

        public void RemoveFormat(string formatName) {
            if(string.IsNullOrWhiteSpace(formatName)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(formatName));
            }

            ThrowWin32Exception(
                SafeExecute(handle => WinSpool.DeleteForm(handle, formatName)));
            Load();
        }

        private void AddFormat(string formatName, Size formatSize, Rectangle visibleArea) {
            WinSpool.FORM_INFO_2 formInfo2 = CreateFormInfo(formatName, formatSize, visibleArea);
            ThrowWin32Exception(
                SafeExecute(handle => WinSpool.AddForm(handle, in formInfo2)));
        }

        private static WinSpool.FORM_INFO_2 CreateFormInfo(string formatName, Size formatSize, Rectangle visibleArea) {
            return new WinSpool.FORM_INFO_2() {
                pName = formatName,
                pKeyword = formatName,
                Flags = WinSpool.FormFlags.FORM_USER,
                StringType = WinSpool.FormStringType.STRING_NONE,
                Size = new SIZE(
                    formatSize.Width * _micronsPerMm,
                    formatSize.Height * _micronsPerMm
                ),
                ImageableArea = new RECT(
                    visibleArea.Left * _micronsPerMm,
                    visibleArea.Top * _micronsPerMm,
                    visibleArea.Right * _micronsPerMm,
                    visibleArea.Bottom * _micronsPerMm
                )
            };
        }

        private T SafeExecute<T>(Func<WinSpool.SafeHPRINTER, T> action) {
            using WinSpool.SafeHPRINTER printerHandle = OpenPrinter2(_printerName);
            try {
                return action(printerHandle);
            } finally {
                ThrowWin32Exception(WinSpool.ClosePrinter(printerHandle));
            }
        }

        private static WinSpool.SafeHPRINTER OpenPrinter2(string printerName) {
            var pDefault = new WinSpool.PRINTER_DEFAULTS();
            var pOptions = WinSpool.PRINTER_OPTIONS.Default;

            ThrowWin32Exception(
                WinSpool.OpenPrinter2(printerName, out WinSpool.SafeHPRINTER phPrinter, pDefault, in pOptions));

            return phPrinter;
        }

        private static void ThrowWin32Exception(bool result) {
            if(!result) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
