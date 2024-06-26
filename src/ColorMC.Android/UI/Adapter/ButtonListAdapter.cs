using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI.Adapter;

public class ButtonListAdapter(Context? context, int textViewResourceId, List<string> objects) 
    : ArrayAdapter<string>(context!, textViewResourceId, objects)
{
    public override View GetView(int position, View? convertView, ViewGroup parent)
    {
        var itemView = LayoutInflater.From(parent.Context)!
            .Inflate(textViewResourceId, parent, false)!;

        string data = GetItem(position)!;
        var textView = itemView.FindViewById<TextView>(Resource.Id.text_view)!;
        textView.Text = data;

        return itemView;
    }
}
