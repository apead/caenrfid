using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CaenRfIdSample.Droid
{
    public  class ViewHolderItem : Java.Lang.Object
    {
        public TextView Epc { get; set; }
        public Color TextColor { get; set; }
        public TextView Counter { get; set; }
    }
    
    public class RfidTagAdapter : ArrayAdapter<RfidTag> {

    private List<RfidTag> _items;

        ViewHolderItem _viewHolder;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LinearLayout view = (LinearLayout)convertView;
            if (view == null)
            {
                LayoutInflater vi = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
                view = (LinearLayout)vi.Inflate(Resource.Layout.inventory_row,
                        null);

                _viewHolder = new ViewHolderItem();
                _viewHolder.Epc = (TextView)view
                        .FindViewById(Resource.Id.inventory_item);
                _viewHolder.Counter = (TextView)view
                        .FindViewById(Resource.Id.inventory_item_counter);

                view.Tag = _viewHolder;
            }
            else
            {
                _viewHolder = (ViewHolderItem)view.Tag;
            }
            var item =  _items[position];
            if (item != null)
            {
                if (_viewHolder != null)
                {
                    _viewHolder.Epc.Text = item.TagId;
                    _viewHolder.Counter.Text =(item.Counter.ToString());
                }
            }
            return view;
        }

        public override void Clear()
        {
            base.Clear();
            _items.Clear();
        }


        public override int Count => _items.Count;


    public RfidTag GetItem(int position)
        {
            return _items[position];
        }

    public int GetPosition(RfidTag item)
        {
            return _items.IndexOf(item);
        }

        public void AddTag(RfidTag rfidTag, short maxRssi, short minRssi)
        {
            var itemIndex = -1;

            for (int i = 0; i < _items.Count; i++)
            {
                var tag = _items[i];

                if (tag != null)
                {
                    if (rfidTag.TagId == tag.TagId)
                    {
                        itemIndex = i;
                        break;
                    }
                }
            }
            if (itemIndex == -1)
                _items.Add(rfidTag);
            else
            {
                _items[itemIndex].Counter = _items[itemIndex].Counter + 1;
            }
            NotifyDataSetChanged();
        }

        public RfidTagAdapter(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public RfidTagAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
        {
            _items = new List<RfidTag>();
        }

        public RfidTagAdapter(Context context, int resource, int textViewResourceId) : base(context, resource, textViewResourceId)
        {
        }

        public RfidTagAdapter(Context context, int textViewResourceId, RfidTag[] objects) : base(context, textViewResourceId, objects)
        {
        }

        public RfidTagAdapter(Context context, int resource, int textViewResourceId, RfidTag[] objects) : base(context, resource, textViewResourceId, objects)
        {
        }

        public RfidTagAdapter(Context context, int textViewResourceId, IList<RfidTag> objects) : base(context, textViewResourceId, objects)
        {
        }

        public RfidTagAdapter(Context context, int resource, int textViewResourceId, IList<RfidTag> objects) : base(context, resource, textViewResourceId, objects)
        {
        }

    }
}