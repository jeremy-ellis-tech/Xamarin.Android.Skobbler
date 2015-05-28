using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.Util;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class POICategoriesListActivity : Activity
    {

        private ListView _listView;

        private POICategoryListAdapter _adapter;

        private List<POICategoryListItem> _listItems;

        private List<int> _selectedCategories = new List<int>();

        private class POICategoryListItem
        {
            public bool IsMainCategory { get; private set; }
            public string Name { get; private set; }
            public int Id { get; private set; }

            public POICategoryListItem(bool isMainCategory, string name, int id)
            {
                IsMainCategory = isMainCategory;
                Name = name;
                Id = id;
            }

            public override string ToString()
            {
                return "[isMainCategory=" + IsMainCategory + ", name=" + Name + ", id=" + Id + "]";
            }
        }

        private static List<POICategoryListItem> GetListItems()
        {
            List<POICategoryListItem> listItems = new List<POICategoryListItem>();
            foreach (SKCategories.SKPOIMainCategory mainCategory in SKCategories.SKPOIMainCategory.Values())
            {
                listItems.Add(new POICategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1));
                foreach (int categoryId in SKUtils.GetSubcategoriesForCategory(mainCategory.Value))
                {
                    listItems.Add(new POICategoryListItem(false, SKUtils.GetMainCategoryForCategory(categoryId).GetNames()[0].ToUpper().Replace("_", " "), categoryId));
                }
            }
            return listItems;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_list);

            FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

            _listItems = GetListItems();

            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _listView.Visibility = ViewStates.Visible;

            _adapter = new POICategoryListAdapter(this);
            _listView.Adapter = _adapter;

            Toast.MakeText(this, "Select the desired POI categories for heat map display", ToastLength.Short).Show();

            _listView.ItemClick += (s, e) =>
            {
                POICategoryListItem selectedItem = _listItems[e.Position];
                if (selectedItem.Id > 0)
                {
                    if (_selectedCategories.Contains(selectedItem.Id))
                    {
                        _selectedCategories.Remove(selectedItem.Id);
                        e.View.SetBackgroundColor(Resources.GetColor(Resource.Color.white));
                    }
                    else
                    {
                        _selectedCategories.Add(selectedItem.Id);
                        e.View.SetBackgroundColor(Resources.GetColor(Resource.Color.selected));
                    }

                    Button showButton = FindViewById<Button>(Resource.Id.show_heat_map);
                    if (_selectedCategories.Count == 0)
                    {
                        showButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        showButton.Visibility = ViewStates.Visible;
                    }
                }
            };
        }

        [Export("onClick")]
        public void OnClick(View v)
        {
            //if (v.Id == Resource.Id.show_heat_map)
            //{
            //    SKPOICategory[] categories = new SKPOICategory[selectedCategories.Count];
            //    for (int i = 0; i < selectedCategories.Count; i++)
            //    {
            //        categories[i] = SKPOICategory.ForInt(selectedCategories[i]);
            //    }
            //    MapActivity.HeatMapCategories = categories;
            //    Finish();
            //}

            if (v.Id == Resource.Id.show_heat_map)
            {
                MapActivity.HeatMapCategories = _selectedCategories.Select(x => SKCategories.SKPOICategory.ForInt(x)).ToArray();
                Finish();
            }
        }

        private class POICategoryListAdapter : BaseAdapter<POICategoryListItem>
        {
            private readonly POICategoriesListActivity _activity;

            public POICategoryListAdapter(POICategoriesListActivity activity)
            {
                _activity = activity;
            }

            public override POICategoryListItem this[int position]
            {
                get { return _activity._listItems[position]; }
            }

            public override int Count
            {
                get { return _activity._listItems.Count; }
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                TextView view = null;

                if (convertView == null)
                {
                    view = new TextView(_activity);
                }
                else
                {
                    view = convertView as TextView; ;
                }

                POICategoryListItem item = _activity._listItems[position];

                view.Text = "  " + item.Name;

                if (item.IsMainCategory)
                {
                    view.SetTextAppearance(_activity, Resource.Style.menu_options_group_style);
                    view.SetBackgroundColor(_activity.Resources.GetColor(Resource.Color.grey_options_group));
                }
                else
                {
                    view.SetTextAppearance(_activity, Resource.Style.menu_options_style);

                    if (!_activity._selectedCategories.Contains(item.Id))
                    {
                        view.SetBackgroundColor(_activity.Resources.GetColor(Resource.Color.white));
                    }
                    else
                    {
                        view.SetBackgroundColor(_activity.Resources.GetColor(Resource.Color.selected));
                    }
                }

                return view;
            }

        }
    }
}