using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.Util;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "POICategoriesListActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class POICategoriesListActivity : Activity
    {
        private ListView _listView;
        private POICategoryListAdapter _adapter;
        private List<POICategoryListItem> _listItems;
        private List<int> _selectedCategories = new List<int>();

        private class POICategoryListItem
        {
            public bool IsMainCategory{get; private set;}
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

        private List<POICategoryListItem> GetListItems()
        {
            List<POICategoryListItem> listItems = new List<POICategoryListItem>();

            foreach (var mainCategory in SKCategories.SKPOIMainCategory.Values())
            {
                listItems.Add(new POICategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1));

                foreach (var categoryId in SKUtils.GetSubcategoriesForCategory(mainCategory.Value))
                {
                    listItems.Add(new POICategoryListItem(false, SKUtils.GetMainCategoryForCategory((int)categoryId).GetNames()[0].ToUpper().Replace("_", " "), (int)categoryId));
                }
            }

            return listItems;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);

            FindViewById<View>(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

            _listItems = GetListItems();

            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _listView.Visibility = ViewStates.Visible;

            _adapter = new POICategoryListAdapter(this, _listItems, _selectedCategories);
            _listView.Adapter = _adapter;

            Toast.MakeText(this, "Select the desired POI categories for heat map display", ToastLength.Short).Show();

            _listView.ItemClick += OnItemClick;
        }

        [Export("OnClick")]
        public void OnClick(View v)
        {
            if(v.Id == Resource.Id.show_heat_map)
            {
                SKCategories.SKPOICategory[] categories = new SKCategories.SKPOICategory[_selectedCategories.Count];
                for (int i = 0; i < _selectedCategories.Count; i++)
                {
                    categories[i] = SKCategories.SKPOICategory.ForInt(_selectedCategories[i]);
                }

                MapActivity.HeatMapCategories = categories;
                Finish();
            }
        }

        void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            POICategoryListItem selectedItem = _listItems[e.Position];
            
            if(selectedItem.Id > 0)
            {
                if(_selectedCategories.Contains(selectedItem.Id))
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
                if(_selectedCategories.Count == 0)
                {
                    showButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    showButton.Visibility = ViewStates.Visible;
                }
            }
        }

        private class POICategoryListAdapter : BaseAdapter<POICategoryListItem>
        {
            private Context _context;
            private List<POICategoryListItem> _listItems;
            private List<int> _selectedCategories;

            public POICategoryListAdapter(Context context, List<POICategoryListItem> listItems, List<int> selectedCategories)
            {
                _context = context;
                _listItems = listItems;
                _selectedCategories = selectedCategories;
            }

            public override int Count
            {
                get { return _listItems.Count; }
            }

            public override POICategoryListItem this[int position]
            {
                get { return _listItems[position]; }
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                TextView view = null;

                if(convertView == null)
                {
                    view = new TextView(_context);
                }
                else
                {
                    view = convertView as TextView;
                }

                POICategoryListItem item = _listItems[position];

                view.Text = " " + item.Name;

                if(item.IsMainCategory)
                {
                    view.SetTextAppearance(_context, Resource.Style.menu_options_group_style);
                    view.SetBackgroundColor(_context.Resources.GetColor(Resource.Color.grey_options_group));
                }
                else
                {
                    view.SetTextAppearance(_context, Resource.Style.menu_options_style);
                    if(!_selectedCategories.Contains(item.Id))
                    {
                        view.SetBackgroundColor(_context.Resources.GetColor(Resource.Color.white));
                    }
                    else
                    {
                        view.SetBackgroundColor(_context.Resources.GetColor(Resource.Color.selected));
                    }
                }

                return view;
            }
        }
    }
}