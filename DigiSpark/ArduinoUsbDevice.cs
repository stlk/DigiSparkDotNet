/* ***********************************************
* File:    ArduinoUsbDevice.cs
* Version: 20130120
* Author:  tinozeegerman@gmail.com
* License: CC-BY-SA (http://freedomdefined.org/Licenses/CC-BY-SA)
* 
* Description: This is pretty much a straight port of usbdevice.py from the DigiSpark Sample Code
*              I've added notification for when the DigiSpark is connected and disconnected.
*              You can use this code to communicate with your DigiSpark's DigiUSB interface
*              To run successfully you need to install LibUsbDotNet: http://sourceforge.net/projects/libusbdotnet/
*              Note that there seems to be a problem with LibDotNetUsb and Notifications on .Net 4.0,
*              use 3.5 or below
*              
* ***********************************************/

using System;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;

namespace DigiSparkDotNet
{
    public class ArduinoUsbDevice : IDisposable
    {
        private readonly UsbDeviceFinder _myUsbFinder;

        private readonly int _productId;
        private readonly int _vendorId;
        private readonly IDeviceNotifier _usbDeviceNotifier;

        private UsbDevice _usbDevice;

        public bool IsAvailable { get; private set; }

        //default values for the DigiSpark
        public ArduinoUsbDevice()
            : this(0x16c0, 0x05df)
        {
        }

        public ArduinoUsbDevice(int vendorId, int productId)
        {
            _vendorId = vendorId;
            _productId = productId;

            _myUsbFinder = new UsbDeviceFinder(_vendorId, _productId);
            _usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

            _usbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

            ConnectUsbDevice();
        }



        public event EventHandler<EventArgs> ArduinoUsbDeviceChangeNotifier;

        private void ConnectUsbDevice()
        {
            _usbDevice = UsbDevice.OpenUsbDevice(_myUsbFinder);

            if (_usbDevice != null)
            {
                IsAvailable = true;

                if (ArduinoUsbDeviceChangeNotifier != null)
                    ArduinoUsbDeviceChangeNotifier.Invoke(true, null);
            }
            else
            {
                IsAvailable = false;
            }
        }


        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if (e.Device.IdVendor == _vendorId && e.Device.IdProduct == _productId)
            {
                if (e.EventType == EventType.DeviceArrival)
                {
                    ConnectUsbDevice();
                }

                else if (e.EventType == EventType.DeviceRemoveComplete)
                {
                    _usbDevice = null;

                    IsAvailable = false;

                    if (ArduinoUsbDeviceChangeNotifier != null)

                        ArduinoUsbDeviceChangeNotifier.Invoke(false, null);
                }
            }
        }


        public string GetStringDescriptor(byte index)
        {
            if (IsAvailable == false)
                return null;

            var packet = new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn,
                (byte)UsbStandardRequest.GetDescriptor,
                (short)(0x0300 | index), // (usb.util.DESC_TYPE_STRING << 8) | index
                0, //Language ID
                255); //Length


            var byteArray = new byte[256];
            int numBytesTransferred;

            _usbDevice.ControlTransfer(ref packet, byteArray, byteArray.Length, out numBytesTransferred);

            return Encoding.Unicode.GetString(byteArray);
        }


        public bool WriteByte(byte value)
        {
            if (IsAvailable == false)
                return false;

            var packet = new UsbSetupPacket(
                (byte)(UsbCtrlFlags.RequestType_Class | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.Direction_Out),
                0x09, // USBRQ_HID_SET_REPORT 
                0x300, // (USB_HID_REPORT_TYPE_FEATURE << 8) | 0,
                value, // the byte to write
                0); // according to usbdevice.py this is ignored, so passing in 0

            int numBytesTransferred;

            return _usbDevice.ControlTransfer(ref packet, null, 0, out numBytesTransferred);
        }


        public bool WriteBytes(byte[] values)
        {
            if (IsAvailable == false)
                return false;

            bool result = true;

            foreach (byte value in values)
            {
                result &= WriteByte(value);
            }

            return result;
        }

        public bool ReadByte(out byte[] value)
        {
            value = new byte[1];

            if (IsAvailable == false)
                return false;

            var packet = new UsbSetupPacket(
                (byte)(UsbCtrlFlags.RequestType_Class | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.Direction_In),
                0x01, // USBRQ_HID_GET_REPORT 
                0x300, // (USB_HID_REPORT_TYPE_FEATURE << 8) | 0,
                0, // according to usbdevice.py this is ignored, so passing in 0
                1); // length


            int numBytesTransferred;

            bool sendResult = _usbDevice.ControlTransfer(ref packet, value, 1, out numBytesTransferred);

            return sendResult & (numBytesTransferred > 0);
        }

        #region Dispose implementation

        private bool _disposed;

        ~ArduinoUsbDevice()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed && _usbDeviceNotifier != null)
            {
                _usbDeviceNotifier.OnDeviceNotify -= OnDeviceNotifyEvent;
            }

            _disposed = true;
        }

        #endregion
    }
}