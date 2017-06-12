using System;
using Android.Bluetooth;
using Android.Content;

namespace CaenRfIdSample.Droid
{
    [BroadcastReceiver]
    public class BtReceiverStart : BroadcastReceiver
    {
        public EventHandler DiscoveryStarted;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;

            if (BluetoothAdapter.ActionDiscoveryStarted == action)
            {
                if (DiscoveryStarted != null)
                {
                    DiscoveryStarted(this, null);
                }
            }
        }
    }
}