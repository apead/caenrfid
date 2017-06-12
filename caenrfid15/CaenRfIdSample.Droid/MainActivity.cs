using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Com.Caen.RFIDLibrary;
using Java.Util;
using Exception = Java.Lang.Exception;


namespace CaenRfIdSample.Droid
{
    [Activity(Label = "Caen BT RFID Sample (Xamarin)", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private BluetoothAdapter _bluetoothAdapter = null;
        private const int RequestConnectDevice = 1;
        private const int RequestEnableBt = 2;
        protected const int DoInventory = 2;
        private const string TAG = "MainActivity";
        private UUID _myUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        protected const int AddReaderBt = 1;
        private ProgressDialog _tcpBtWaitProgressDialog;
        public static List<DemoReader> Readers;
        public static bool ReturnFromActivity = false;

        protected static bool Started = true;
        protected static bool Destroyed = false;
        protected static bool ConnectionSuccesfull = false;

        private IList<IDictionary<string, object>> _data = new List<IDictionary<string, object>>();
        private SimpleAdapter _adapter;
        public static int SelectedReader;

        private BtDisconnectReceiver _btDisconnectReceiver = new BtDisconnectReceiver();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Log.Error(TAG, "+++ ON CREATE +++");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ;

            SetContentView(Resource.Layout.main);

            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (_bluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available", ToastLength.Long).Show();
                Finish();
                return;
            }

            var addReaderButton = FindViewById<Button>(Resource.Id.add_reader_button);

            addReaderButton.Click += DiscoverButton_Click;

            if (!MainActivity.ReturnFromActivity)
            {
                Readers = new List<DemoReader>();
            }
            else
                MainActivity.ReturnFromActivity = false;

            var lv = ((ListView) FindViewById(Resource.Id.reader_list));

            lv.ItemClick += Lv_Click;

            _btDisconnectReceiver.Disconnect += DisconnectedFromBt;

            IntentFilter disconnectFilter = new IntentFilter(BluetoothDevice.ActionAclDisconnected);
            this.RegisterReceiver(_btDisconnectReceiver, disconnectFilter);

            IntentFilter disconnectFilter2 = new IntentFilter(
                BluetoothAdapter.ActionStateChanged);
            this.RegisterReceiver(_btDisconnectReceiver, disconnectFilter2);

        }

        private void DisconnectedFromBt(object sender, EventArgs args)
        {
            var pos = 0;

            List<int> removeList = new List<int>();

            foreach (var demoReader in Readers)
            {
                try
                {
                    _data.RemoveAt(pos);
                    _adapter.NotifyDataSetChanged();
                    demoReader.GetReader().Disconnect();
                    removeList.Add(pos);
                }
                catch (CAENRFIDException e)
                {
                    Log.Error(TAG, "BT Disconnect " + e.Message);
                }

                pos++;
            }

            foreach (int t in removeList)
                Readers.RemoveAt(t);

            if (removeList.Count != 0)
            {
                Toast.MakeText(ApplicationContext,
                    "Bluetooth device disconnected!",
                    ToastLength.Short).Show();
            }
        }
 

private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(TAG, "++ Unhandled Exception ++ " + e.ExceptionObject.ToString());
        }

        protected override void OnStart()
        {
            base.OnStart();

            Log.Error(TAG, "++ ON START ++");

            Started = true;
            Destroyed = false;

        }

        private void Lv_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedReader = e.Position;

            Intent inventory = new Intent(ApplicationContext,
                typeof(InventoryActivity));

            StartActivityForResult(inventory, DoInventory);
        }

        private void DiscoverButton_Click(object sender, System.EventArgs e)
        {
            Intent addReader = new Intent(ApplicationContext, typeof(BtSelectionActivity));
            StartActivityForResult(addReader, AddReaderBt);

        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            string ip = null;
            BluetoothDevice dev = null;
            switch (requestCode)
            {
                case AddReaderBt:
                    if (resultCode == Result.Ok)
                    {
                        dev = (BluetoothDevice) ((Intent) data).GetParcelableExtra("BT_DEVICE");

                        _tcpBtWaitProgressDialog = ProgressDialog.Show(this,
                            "Connection ", "Connecting to " + dev.Name, true,
                            true);

                        await Task.Run(() =>
                        {

                            BluetoothSocket sock;
                            CAENRFIDReader reader = null;
                            CAENRFIDReaderInfo info = null;
                            string fwrel = null;

                            bool no_connection = true;

                            try
                            {
                                sock = dev.CreateRfcommSocketToServiceRecord(_myUuid);

                                reader = new CAENRFIDReader();


                                reader.Connect(sock);
                                var state = dev.BondState;

                                while (state != Bond.Bonded)
                                    state = dev.BondState;


                                info = reader.ReaderInfo;
                                fwrel = reader.FirmwareRelease;

                                DemoReader dr = new DemoReader(reader, info.Model,
                                    info.SerialNumber, fwrel, CAENRFIDPort.CaenrfidBt);
                                Readers.Add(dr);

                            }
                            catch (CAENRFIDException e)
                            {
                                Log.Error(TAG, "Exception " + e.Message);

                                Toast.MakeText(ApplicationContext, "Failed" , ToastLength.Long);
                            }
                        });


                        UpdateReadersList();
                        _tcpBtWaitProgressDialog.Dismiss();
                    }
                    break;
            }
        }

        private void UpdateReadersList()
        {
            if (Readers != null)
            {
                CAENRFIDPort isTCP = null;
                ((ListView) FindViewById(Resource.Id.reader_list)).Adapter = null;
                _data.Clear();

                for (int i = 0; i < Readers.Count; i++)
                {
                    DemoReader r = Readers[i];

                    var readerMap = new JavaDictionary<string, object>();
                    isTCP = r.ConnectionType;
                    readerMap["image"] = isTCP == CAENRFIDPort.CaenrfidTcp
                        ? Resource.Drawable.ic_tcp_reader
                        : Resource.Drawable.ic_bt_reader;
                    readerMap["name"] = r.ReaderName;
                    readerMap["info"] = "Serial: " + r.Serial
                                        + "\nFirmware: " + r.FirmwareRelease
                                        + "\nRegulation: " + r.GetRegulation();
                    _data.Add(readerMap);
                }
            }

            string[] from = {"image", "name", "info"};
            int[] to = {Resource.Id.reader_image, Resource.Id.reader_name, Resource.Id.reader_info};

            _adapter = new SimpleAdapter(ApplicationContext, _data,
                Resource.Layout.list_reader, from, to);

            ((ListView) FindViewById(Resource.Id.reader_list)).Adapter = _adapter;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var demoReader in Readers)
            {
                try
                {

                    demoReader.GetReader().Disconnect();

                }
                catch (CAENRFIDException e)
                {
                    Log.Error(TAG, "On Destroy " + e.Message);
                }
            }
            Readers = null;

            UnregisterReceiver(_btDisconnectReceiver);
        }
    }
}

