using FTDI.D2xx.WinRT;
using FTDI.D2xx.WinRT.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace FTDISimple
{
    static class FTDISimple
    {
        private static ThreadPoolTimer timer;
        private static FTManager ftManager;
        private static bool deviceNotStarted;
        public static int Timeout { get; set; } = 10000;
        public static int TimerPeriod { get; set; } = 500;

        public static void Init()
        {
            deviceNotStarted = true;
            InitFTDI();
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(TimerPeriod));
        }

        private static void InitFTDI()
        {
            try
            {
                ftManager = new FTManager();
                string ver = FTManager.GetLibraryVersion();
                Debug.WriteLine(ver);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        
        public static void StartDevice()
        {
            deviceNotStarted = false;
            IList<IFTDeviceInfoNode> deviceList = ftManager.GetDeviceList();
            foreach (IFTDeviceInfoNode deviceInfo in deviceList)
            {
                Debug.WriteLine("Device Type: {0}\r\nSerial Number: {1}\r\nDescription: {2}\r\n\r\n ", deviceInfo.DeviceType.ToString(), deviceInfo.SerialNumber, deviceInfo.Description);
                if (deviceInfo.Description == "My USB Product")
                {
                }
            }
        }

        public static async void OpenDevice(IFTDeviceInfoNode deviceInfo)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(Timeout);
            var myDevice = ftManager.OpenByDescription(deviceInfo.Description);
            await myDevice.SetBaudRateAsync(9600);
            await myDevice.SetFlowControlAsync(FLOW_CONTROL.RTS_CTS, 0x00, 0x00);
            await myDevice.SetDataCharacteristicsAsync(WORD_LENGTH.BITS_8, STOP_BITS.BITS_1, PARITY.NONE);
            await myDevice.SetLatencyTimerAsync(16);
            var action = ThreadPool.RunAsync(async (source) => {
                byte[] dataTx = new byte[10]; for (int i = 0; i < dataTx.Length; i++) dataTx[i] = (byte)(i + 60);
                dataTx[7] = (byte)0;
                dataTx[8] = (byte)'\r';
                dataTx[9] = (byte)'\n';
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    byte[] dataRx = new byte[10];
                    await myDevice.WriteAsync(dataTx, 10);
                    await myDevice.ReadAsync(dataRx, 10);
                }
            }, WorkItemPriority.Normal);
        }


        private static void Timer_Tick(ThreadPoolTimer timer)
        {
            //value = (value == GpioPinValue.High) ? GpioPinValue.Low : GpioPinValue.High;
            // pin.Write(value);
            if (ftManager != null)
            {
                try
                {
                    IList<IFTDeviceInfoNode> deviceList = ftManager.GetDeviceList();
                    if (deviceNotStarted)
                        if (deviceList.Count != 0)
                            StartDevice();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
