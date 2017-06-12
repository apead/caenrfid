using System;
using Android.Bluetooth;
using Android.Content;

namespace CaenRfIdSample.Droid
{
    [BroadcastReceiver]
    public class BtBroadcastReceiver : BroadcastReceiver
    {
        public EventHandler DeviceReceived;

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            if (BluetoothDevice.ActionFound == action)
            {
                BluetoothDevice deviceBt = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;

                if (deviceBt != null)
                    DeviceReceived?.Invoke(deviceBt,new EventArgs());
            }
        }
    }
}