using System;
using System.Collections.Generic;
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
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class PoiCategoriesListActivity : Activity
    {

        private ListView _listView;

        private PoiCategoryListAdapter _adapter;

        private IList<PoiCategoryListItem> _listItems;

        private IList<int?> _selectedCategories = new List<int?>();

        private class PoiCategoryListItem
        {

            internal bool IsMainCategory;

            internal string Name;

            internal int Id;

            public PoiCategoryListItem(bool isMainCategory, string name, int id)
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

        private static IList<PoiCategoryListItem> ListItems
        {
            get
            {
                IList<PoiCategoryListItem> listItems = new List<PoiCategoryListItem>();
                foreach (SKCategories.SKPOIMainCategory mainCategory in SKCategories.SKPOIMainCategory.Values())
                {
                    listItems.Add(new PoiCategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1));
                    foreach (int categoryId in SKUtils.GetSubcategoriesForCategory(mainCategory.Value))
                    {
                        listItems.Add(new PoiCategoryListItem(false, SKUtils.GetMainCategoryForCategory(categoryId).GetNames()[0].ToUpper().Replace("_", " "), categoryId));
                    }
                }
                return listItems;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

            _listItems = ListItems;

            _listView = (ListView)FindViewById(Resource.Id.list_view);
            _listView.Visibility = ViewStates.Visible;

            _adapter = new PoiCategoryListAdapter(this);
            _listView.Adapter = _adapter;

            Toast.MakeText(this, "Select the desired POI categories for heat map display", ToastLength.Short).Show();

            _listView.ItemClick += (s, e) =>
            {
                PoiCategoryListItem selectedItem = _listItems[e.Position];
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

                    Button showButton = (Button)FindViewById(Resource.Id.show_heat_map);
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

        [Export("OnClick")]
        public virtual void OnClick(View v)
        {
            if (v.Id == Resource.Id.show_heat_map)
            {
                SKCategories.SKPOICategory[] categories = new SKCategories.SKPOICategory[_selectedCategories.Count];
                for (int i = 0; i < _selectedCategories.Count; i++)
                {
                    categories[i] = SKCategories.SKPOICategory.ForInt(_selectedCategories[i].Value);
                }
                MapActivity.HeatMapCategories = categories;
                Finish();
            }
        }

        private class PoiCategoryListAdapter : BaseAdapter<PoiCategoryListItem>
        {
            private readonly PoiCategoriesListActivity _outerInstance;

            public PoiCategoryListAdapter(PoiCategoriesListActivity outerInstance)
            {
                _outerInstance = outerInstance;
            }


            public override int Count
            {
                get
                {
                    return _outerInstance._listItems.Count;
                }
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
                    view = new TextView(_outerInstance);
                }
                else
                {
                    view = (TextView)convertView;
                }

                PoiCategoryListItem item = _outerInstance._listItems[position];

                view.Text = "  " + item.Name;
                if (item.IsMainCategory)
                {
                    view.SetTextAppearance(_outerInstance, Resource.Style.menu_options_group_style);
                    view.SetBackgroundColor(_outerInstance.Resources.GetColor(Resource.Color.grey_options_group));
                }
                else
                {
                    view.SetTextAppearance(_outerInstance, Resource.Style.menu_options_style);
                    if (!_outerInstance._selectedCategories.Contains(item.Id))
                    {
                        view.SetBackgroundColor(_outerInstance.Resources.GetColor(Resource.Color.white));
                    }
                    else
                    {
                        view.SetBackgroundColor(_outerInstance.Resources.GetColor(Resource.Color.selected));
                    }
                }
                return view;
            }

            public override PoiCategoryListItem this[int position]
            {
                get { return _outerInstance._listItems[position]; }
            }
        }
    }

}