using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Com.Caen.RFIDLibrary;

namespace CaenRfIdSample.Droid
{
    [Activity(Label = "Inventory")]
    public class InventoryActivity : Activity, ICAENRFIDEventListener
    {
        public const int InitRssiValue = -1;
        public static Dictionary<string, int> TagListPosition;
        public static int SelectedTag;

        protected DemoReader Reader;
        protected RfidTagAdapter _rFIDTagAdapter;
        protected Button _buttonInventory;
        protected ListView _inventoryList;
        protected TextView _totalFound;
        protected TextView _currentFound;

        protected int ReaderPosition = 0;
        protected short MaxRssi = 0;
        protected short MinRssi = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.inventory_selection);

            _rFIDTagAdapter = new RfidTagAdapter(ApplicationContext,Resource.Id.inventory_list);

            _inventoryList = (ListView)this.FindViewById(Resource.Id.inventory_list);
            _inventoryList.Adapter = _rFIDTagAdapter;


            Reader = MainActivity.Readers[MainActivity.SelectedReader];
            _totalFound = (TextView)this.FindViewById(Resource.Id.total_found_num);

            StartReceivingTags();

        }

        protected override void OnStart()
        {
            base.OnStart();
            var reader = Reader.GetReader();

            reader.AddCAENRFIDEventListener(this);

        }

        protected override void OnStop()
        {
            base.OnStop();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var reader = Reader.GetReader();
            reader.InventoryAbort();
        }

        private void StartReceivingTags()
        {
            CAENRFIDReader reader = Reader.GetReader();

            CAENRFIDLogicalSource source = null;
            source = reader.GetSource("Source_0");
            source.ReadCycle = 0;

            source.Selected_EPC_C1G2 = CAENRFIDLogicalSourceConstants.EPCC1G2AllSELECTED;
            source
                .EventInventoryTag(
                    new byte[0],
                    (short) 0x0,
                    (short) 0x0,
                    (short) 0x7);
        }

        public void CAENRFIDTagNotify(CAENRFIDEvent p0)
        {
            var tag = (CAENRFIDNotify) p0.Data[0];
            var tmpRssi = tag.RSSI;

            if (MaxRssi == InitRssiValue)
            {
                MaxRssi = tmpRssi;
                MinRssi = tmpRssi;
            }
            if (tmpRssi > MaxRssi)
                MaxRssi = tmpRssi;
            else if (tmpRssi < MinRssi)
                MinRssi = tmpRssi;

            RunOnUiThread(() =>
            {
                _rFIDTagAdapter.AddTag(new RfidTag(tag, Color.Blue, RfidTag.ToHexString(tag.GetTagID()), tag.RSSI),
                    MaxRssi, MinRssi);
                _totalFound.Text = _rFIDTagAdapter.Count.ToString(); 
            });
        }
    }
}