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
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class PoiCategoriesListActivity : Activity
    {

        private ListView _listView;

        private POICategoryListAdapter _adapter;

        private IList<POICategoryListItem> _listItems;

        private readonly IList<int> _selectedCategories = new List<int>();

        private class POICategoryListItem
        {
            public POICategoryListItem(bool isMainCategory, string name, int id)
            {
                IsMainCategory = isMainCategory;
                Name = name;
                Id = id;
            }

            public bool IsMainCategory { get; private set; }
            public string Name { get; private set; }
            public int Id { get; private set; }

            public override string ToString()
            {
                return "[isMainCategory=" + IsMainCategory + ", name=" + Name + ", id=" + Id + "]";
            }
        }

        private static IEnumerable<POICategoryListItem> GetListItems()
        {
            foreach (var mainCategory in SKCategories.SKPOIMainCategory.Values())
            {
                yield return new POICategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1);

                foreach (int categoryId in SKUtils.GetSubcategoriesForCategory(mainCategory.Value))
                {
                    yield return new POICategoryListItem(false, SKUtils.GetMainCategoryForCategory(categoryId).GetNames()[0].ToUpper().Replace("_", " "), categoryId);
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

            _listItems = GetListItems().ToList();

            _listView = (ListView)FindViewById(Resource.Id.list_view);
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

                    Button showButton = (Button)FindViewById(Resource.Id.show_heat_map);
                    showButton.Visibility = _selectedCategories.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
                }
            };
        }

        [Export("OnClick")]
        public virtual void OnClick(View v)
        {
            if (v.Id != Resource.Id.show_heat_map) return;
            MapActivity.HeatMapCategories = _selectedCategories.Select(SKCategories.SKPOICategory.ForInt).ToArray();
            Finish();
        }

        private class POICategoryListAdapter : BaseAdapter<POICategoryListItem>
        {
            private readonly PoiCategoriesListActivity _outerInstance;

            public POICategoryListAdapter(PoiCategoriesListActivity outerInstance)
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

                POICategoryListItem item = _outerInstance._listItems[position];

                view.Text = "  " + item.Name;
                if (item.IsMainCategory)
                {
                    view.SetTextAppearance(_outerInstance, Resource.Style.menu_options_group_style);
                    view.SetBackgroundColor(_outerInstance.Resources.GetColor(Resource.Color.grey_options_group));
                }
                else
                {
                    view.SetTextAppearance(_outerInstance, Resource.Style.menu_options_style);
                    view.SetBackgroundColor(!_outerInstance._selectedCategories.Contains(item.Id)
                        ? _outerInstance.Resources.GetColor(Resource.Color.white)
                        : _outerInstance.Resources.GetColor(Resource.Color.selected));
                }
                return view;
            }

            public override POICategoryListItem this[int position]
            {
                get { return _outerInstance._listItems[position]; }
            }
        }
    }

}