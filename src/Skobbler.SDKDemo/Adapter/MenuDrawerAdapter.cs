using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Skobbler.SDKDemo.Model;
using JavaObject = Java.Lang.Object;

namespace Skobbler.SDKDemo.Adapter
{
    public class MenuDrawerAdapter : ArrayAdapter<MenuDrawerItem>
    {
        private LayoutInflater _inflater;
        private List<MenuDrawerItem> _objects;

        public MenuDrawerAdapter(Context context, int textViewResourceId, List<MenuDrawerItem> objects) : base(context, textViewResourceId, objects)
        {
            _inflater = LayoutInflater.From(context);
            _objects = objects;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view;
            MenuDrawerItem menuItem = GetItem(position);

            view = getItemView(convertView, parent, menuItem);

            return view;
        }

        public View getItemView(View convertView, ViewGroup parentView, MenuDrawerItem navDrawerItem)
        {
            MenuDrawerItem menuItem = (MenuDrawerItem)navDrawerItem;

            MenuItemHolder navMenuItemHolder = null;

            TextView labelView;

            if (convertView == null)
            {
                if (menuItem.ItemType == MenuDrawerItem.ItemTypeType)
                {
                    convertView = _inflater.Inflate(Resource.Layout.element_menu_drawer_item, parentView, false);
                    labelView = convertView.FindViewById<TextView>(Resource.Id.navmenu_item_label);
                }
                else
                {
                    convertView = _inflater.Inflate(Resource.Layout.element_menu_drawer_section, parentView, false);
                    labelView = convertView.FindViewById<TextView>(Resource.Id.navmenusection_label);
                }


                navMenuItemHolder = new MenuItemHolder();
                navMenuItemHolder.LabelView = labelView;


                convertView.Tag = navMenuItemHolder;
            }

            if (navMenuItemHolder == null)
            {
                navMenuItemHolder = (MenuItemHolder)convertView.Tag;
            }

            navMenuItemHolder.LabelView.Text = menuItem.Label;


            return convertView;
        }

        public override int ViewTypeCount
        {
            get
            {
                return _objects.Count;
            }
        }

        public override int GetItemViewType(int position)
        {
            return GetItem(position).ItemType;
        }

        private class MenuItemHolder : JavaObject
        {
            public TextView LabelView { get; set; }
        }
    }
}