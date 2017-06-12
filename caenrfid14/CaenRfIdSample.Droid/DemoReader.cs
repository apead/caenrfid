using System;
using Android.OS;
using Com.Caen.RFIDLibrary;

namespace CaenRfIdSample.Droid
{
    public class DemoReader : IParcelable
    {
        private CAENRFIDReader _reader;
        public string ReaderName { get; set; }
        public string Serial { get; set; }
        public string FirmwareRelease { get; set; }

        private string regulation;

        public CAENRFIDPort ConnectionType { get; set; }

        public static readonly int EVENT_CONNECTED = 10;
        public static readonly int EVENT_DISCONNECT = 20;

        public DemoReader(CAENRFIDReader caenReader, string readerName, string serialNum, string fwRel,
            CAENRFIDPort connType)
        {
            _reader = caenReader;
            ReaderName = readerName;
            Serial = serialNum;
            FirmwareRelease = fwRel;

            try
            {
                 SetRegulation(_reader.RFRegulation);
            }
            catch (CAENRFIDException e)
            {
            }

            ConnectionType = connType;

        }

        public void Dispose()
        {
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            Java.Lang.Object[] array = new Java.Lang.Object[4];
            array[0] = _reader;
            array[1] = ReaderName;
            array[2] = Serial;
            array[3] = FirmwareRelease;
            dest.WriteArray(array);
        }

        private DemoReader(Parcel parcel)
        {
            _reader = (CAENRFIDReader) parcel.ReadValue(Java.Lang.Class.FromType(typeof(DemoReader)).ClassLoader);
            ReaderName = parcel.ReadString();
            Serial = parcel.ReadString();
            FirmwareRelease = parcel.ReadString();
        }

        public IntPtr Handle { get; }

        public string GetRegulation()
        {

            return regulation;
        }
        public void SetRegulation(CAENRFIDRFRegulations reg)
        {
            if (CAENRFIDRFRegulations.Australia == reg)
                regulation = "AUSTRALIA";
            else if (CAENRFIDRFRegulations.Brazil == reg)
                regulation = "BRAZIL";
            else if (CAENRFIDRFRegulations.China == reg)
                regulation = "CHINA";
            else if (CAENRFIDRFRegulations.Etsi300220 == reg)
                regulation = "ETSI 300220";
            else if (CAENRFIDRFRegulations.Etsi302208 == reg)
                regulation = "ETSI 302208";
            else if (CAENRFIDRFRegulations.FccUs == reg)
                regulation = "FCC_US";
            else if (CAENRFIDRFRegulations.Japan == reg)
                regulation = "JAPAN";
            else if (CAENRFIDRFRegulations.Korea == reg)
                regulation = "KOREA";
            else if (CAENRFIDRFRegulations.Malaysia == reg)
                regulation = "MALAYSIA";
            else if (CAENRFIDRFRegulations.Singapore == reg)
                regulation = "SINGAPORE";
            else if (CAENRFIDRFRegulations.Taiwan == reg)
                regulation = "TAIWAN";
            else
                regulation = "UNKNOWN";
        }

        public CAENRFIDReader GetReader()
        {
            return _reader;
        }
    }
}