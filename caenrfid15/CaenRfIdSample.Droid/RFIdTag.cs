using System.Text;
using Com.Caen.RFIDLibrary;
using Java.Lang;
using String = System.String;
using StringBuilder = System.Text.StringBuilder;

namespace CaenRfIdSample.Droid
{
    public class RfidTag
    {
        public CAENRFIDNotify Tag { get; set; }
        public int TagColor { get; set; }
        public String TagId { get; set; }
        public short Rssi { get; set; }
        public int Counter { get; set; }
        public string Ascii { get; set; }

        protected const int MAX_PASS = 256;
        protected const int MAX_TRANSPARENCY = 110; // 0x00 invisible - 0xff
        // full color
        protected const int MIN_TRANSPARENCY = 200;
        private static int MAX_RETRY = 3;

        /**
         * table to convert a nibble to a hex char.
         */

        static char[] hexChar =
        {
            '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };

        /**
         * Fast convert a byte array to a hex string with possible leading zero.
         * 
         * @param b
         *            array of bytes to convert to string
         * @return hex representation, two chars per byte.
         */

        public static string ToHexString(byte[] b)
        {
            StringBuilder sb = new StringBuilder(b.Length * 2);
            foreach (byte aB in b)
            {
                // look up high nibble char
                sb.Append(hexChar[(aB & 0xf0) >> 4]);
                // look up low nibble char
                sb.Append(hexChar[aB & 0x0f]);
            }
            return sb.ToString();
        }

        public static byte[] HexStringToByteArray(String s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte) ((Character.Digit(s[i], 16) << 4) + Character
                                          .Digit(s[i + 1], 16));
            }
            return data;
        }

        public static string ToAscii(byte[] b)
        {
            return System.Text.Encoding.ASCII.GetString(b).Trim();
        }

        public static byte[] AsciiStringToAsciiByteArray(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        public static byte[] IntToByteArray(int value)
        {
            return new byte[]
            {
                (byte) (value >> 24), (byte) (value >> 16),
                (byte) (value >> 8), (byte) value
            };
        }

        public static byte[] ReadWithRetry(CAENRFIDTag tag, short memBank,
            short address, short length)
        {
            int retry_count = 0;
            bool retry = true;
            byte[] result = null;
            while (retry)
                try
                {
                    result = tag.Source.ReadTagData_EPC_C1G2(tag, memBank,
                        address, length);
                    retry = false;
                }
                catch (CAENRFIDException err)
                {
                    retry_count++;
                    if (retry_count == MAX_RETRY)
                    {
                        retry = false;
                    }
                }
            return result;
        }

        public static byte[] ReadWithRetry(CAENRFIDTag tag, short memBank,
            short address, short length, int password)
        {
            int retry_count = 0;
            bool retry = true;
            byte[] result = null;
            while (retry)
                try
                {
                    result = tag.Source.ReadTagData_EPC_C1G2(tag, memBank,
                        address, length, password);
                    retry = false;
                }
                catch (CAENRFIDException err)
                {
                    retry_count++;
                    if (retry_count == MAX_RETRY)
                    {
                        retry = false;
                    }
                }
            return result;
        }


        public static int GetColor(int baseColor, short rssi, short maxRssi,
            short minRssi)
        {
            int color = baseColor;
            int tmp = 0x00000000;
            int added_transparency = 0;

            if (rssi > maxRssi)
            {
                return baseColor;
            }
            else if (rssi < minRssi)
            {
                return Android.Graphics.Color.Transparent;
            }
            float pass = (float) ((float) (maxRssi - minRssi) / MAX_PASS);
            int i = 0;
            for (i = 0; ((rssi - (pass * i)) >= (minRssi)) && (i < MAX_PASS - 1); i++)
            {
                added_transparency++;
            }
            if (added_transparency < MAX_TRANSPARENCY && added_transparency >= 0)
                added_transparency = MAX_TRANSPARENCY;
            if (added_transparency > MIN_TRANSPARENCY)
                added_transparency = MAX_PASS - 1;
            tmp += added_transparency;
            tmp = tmp << 24;
            color = color & 0x00ffffff; // clean transparency
            color = color | tmp;
            return color;
        }

        public RfidTag(CAENRFIDNotify tag, int color, String Id, short rssi)
        {
            this.Tag = tag;
            this.TagColor = color;
            this.TagId = Id;
            Rssi = rssi;
            Counter = 1;
            Ascii = (ToAscii(Tag.GetTagID()));
        }


        public override string ToString()
        {
            return this.TagId;
        }
    }
}
