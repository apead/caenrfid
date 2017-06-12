using System;
using System.Collections.Generic;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;

namespace CaenRfIdSample.Droid
{
    [Activity(Label = "Bluetooth Selection")]
    public class BtSelectionActivity : Activity
    {
        private static int REQUEST_ENABLE_BT = 1;
        private static int REQUEST_ENABLE_BT_CANCELLED = 10;
        private ProgressDialog _progressDialog;
        private BluetoothDevice deviceBT = null;
        private BluetoothAdapter _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private ArrayAdapter<string> _arrayAdapter = null;
        private List<BluetoothDevice> _arrayDevice = null;
        private ProgressBar _searchProgressBar = null;
        private TextView _searchLabel = null;

        private BtBroadcastReceiver _btReceiver = new BtBroadcastReceiver();
        private BtReceiverStart _btReceiverStart = new BtReceiverStart();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.bt_selection);


            Title = "Choose bluetooth device...";

            _btReceiver.DeviceReceived += OnDeviceReceived;

            _progressDialog = new ProgressDialog(ApplicationContext);


            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(_btReceiver, filter);

            var filter2 = new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted);
            RegisterReceiver(_btReceiverStart, filter2);

            _arrayAdapter = new ArrayAdapter<string>(this,
                Resource.Layout.bt_selection_item);
            _arrayDevice = new List<BluetoothDevice>();

            ListView lv = (ListView) FindViewById(Resource.Id.bt_selection_list);
            lv.Adapter = _arrayAdapter;

            lv.ItemClick += Lv_ItemClick;

            lv.TextFilterEnabled = true;


            if (_bluetoothAdapter == null)
            {
                Toast.MakeText(ApplicationContext, "No bluetooth adapter...",
                    ToastLength.Long).Show();
            }
            else
            {
                if (!_bluetoothAdapter.IsEnabled)
                {
                    var enableBtIntent = new Intent(
                        BluetoothAdapter.ActionRequestEnable);
                    StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);

                }
                RegisterReceiver(_btReceiver, filter);

                var pairedDevices = _bluetoothAdapter.BondedDevices;

                if (pairedDevices.Count > 0)
                {
                    foreach (var device in pairedDevices)
                    {
                        _arrayAdapter.Add(device.Name + "\n"
                                                         + device.Address);
                        _arrayDevice.Add(device);
                    }
                }
                _bluetoothAdapter.StartDiscovery();

            }
        }

        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var dev = _arrayDevice[e.Position];
            Intent newIntent = new Intent();
            newIntent.PutExtra("BT_DEVICE", dev);
            SetResult(Result.Ok, newIntent);

            _progressDialog = ProgressDialog.Show(this, "Connection", "Connecting to " + dev.Name, true, true);
            if (_bluetoothAdapter.IsDiscovering)
                _bluetoothAdapter.CancelDiscovery();
            Finish();
        }

        public void OnDeviceReceived(object sender, EventArgs args)
        {
            var device = sender as BluetoothDevice;

            if (device != null)
            {
                var sdev = device.Name + "\n" + device.Address;
                int ndev = _arrayAdapter.Count;
                string tmp = null;

                for (var i = 0; i < ndev; i++)
                {
                    tmp = _arrayAdapter.GetItem(i);
                    if (tmp.ToLower() == sdev.ToLower())
                        return;
                }
                _arrayAdapter.Add(sdev);
                _arrayDevice.Add(device);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == REQUEST_ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                {

                    var pairedDevices = _bluetoothAdapter.BondedDevices;

                    if (pairedDevices.Count > 0)
                    {
                        foreach (var device in pairedDevices)
                        {
/*                            mArrayAdapter.add(device.getName() + "\n"
                                    + device.getAddress());
                            mArrayDevice.add(device);*/
                        }
                    }
                    _bluetoothAdapter.StartDiscovery();
                }
                else
                {
                    this.SetResult(Result.Canceled);
                    Finish();
                }
            }
        }


    protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_bluetoothAdapter.IsDiscovering)
                _bluetoothAdapter.CancelDiscovery();

            _progressDialog.Dismiss();
            UnregisterReceiver(_btReceiverStart);
            UnregisterReceiver(_btReceiver);
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }
    }
}