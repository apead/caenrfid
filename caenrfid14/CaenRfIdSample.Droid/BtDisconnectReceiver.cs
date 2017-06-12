using System;
using Android.Bluetooth;
using Android.Content;

namespace CaenRfIdSample.Droid
{
    [BroadcastReceiver]
    public class BtDisconnectReceiver : BroadcastReceiver
    {
        public EventHandler Disconnect;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;

            if ((action == (BluetoothAdapter.ActionStateChanged) && (!BluetoothAdapter
                     .DefaultAdapter.IsEnabled)
                 || (action == BluetoothDevice.ActionAclDisconnected)))
            {
                if (Disconnect != null)
                {
                    Disconnect(this,null);
                }
            }
        }
    }
}