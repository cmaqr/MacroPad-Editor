using HidLibrary;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSoft.MacroPad.BLL.Infrasturture.UsbDevice
{
    public class HidLib
    {
        private bool _deviceStatus;
        private List<HidDevice> _deviceList = new List<HidDevice>();
        private HidDevice _hidDevice;

        public ProtocolType? ProtocolType { get; private set; }

        public ushort VendorId { get; private set; }
        public ushort ProductId { get; private set; }

        public bool DeviceStatus => _deviceStatus;


        public bool ConnectDevice(params (ushort VendorId, ushort ProductId, string PathFragment, ProtocolType ProtocolType)[] supportedProducts)
        {
            foreach (var supportedProduct in supportedProducts)
            {
                _hidDevice = HidDevices.Enumerate(supportedProduct.VendorId, supportedProduct.ProductId).FirstOrDefault();
                if (_hidDevice != null)
                {
                    foreach (HidDevice hidDevice in HidDevices.Enumerate(supportedProduct.VendorId).ToList())
                    {
                        if (hidDevice.DevicePath.IndexOf(supportedProduct.PathFragment) != -1)
                        {
                            _deviceList.Add(hidDevice);
                            _hidDevice = hidDevice;
                            _hidDevice.OpenDevice();
                            //// Somehow this is not supported in .net6 but doesn't seem to make any difference
                            //_hidDevice.MonitorDeviceEvents = true;

                            ProtocolType = supportedProduct.ProtocolType;
                            VendorId = supportedProduct.VendorId;
                            ProductId = supportedProduct.ProductId;

                            _deviceStatus = true;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckConnection()
        {
            if (_hidDevice.IsConnected)
                return true;
            _hidDevice.CloseDevice();
            _deviceStatus = false;
            return false;
        }

        public bool WriteDevice(byte reportId, byte[] buffer)
        {
            if (_hidDevice == null)
                return false;

            try
            {
                var report = _hidDevice.CreateReport();
                report.ReportId = reportId;

                // Alguns teclados informam um tamanho de report diferente do nosso buffer.
                // Copiar só o que cabe nos dois evita estourar o índice (issue #34 do projeto original).
                var byteCount = Math.Min(report.Data.Length, buffer.Length);
                for (int i = 0; i < byteCount; ++i)
                    report.Data[i] = buffer[i];

                HidLog.AppendMsg(report.ReportId, ProtocolType == Model.ProtocolType.Legacy ? report.Data.Take(8) : report.Data);

                return _hidDevice.WriteReport(report, 500);
            }
            catch (Exception)
            {
                // Teclado que informa tamanho de report inválido faz a biblioteca HID estourar.
                // Em vez de derrubar o app, a escrita falha e a tela avisa.
                _deviceStatus = false;
                return false;
            }
        }

        /// <summary>
        /// Lista os produtos de um fabricante que estão ligados agora, para identificar macropad
        /// que ainda não está no config.txt (issues #36 e #37 do projeto original).
        /// </summary>
        public static IEnumerable<(ushort ProductId, string Path)> FindConnectedProducts(ushort vendorId)
        {
            return HidDevices.Enumerate(vendorId)
                .Select(device => ((ushort)device.Attributes.ProductId, device.DevicePath))
                .ToList();
        }
    }
}
