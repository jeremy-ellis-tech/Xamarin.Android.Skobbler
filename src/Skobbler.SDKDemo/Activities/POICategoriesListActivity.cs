using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.Util;
using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
	public class POICategoriesListActivity : Activity
	{

		private ListView listView;

		private POICategoryListAdapter adapter;

		private IList<POICategoryListItem> listItems;

		private IList<int?> selectedCategories = new List<int?>();

		private class POICategoryListItem
		{

			internal bool isMainCategory;

			internal string name;

			internal int id;

			public POICategoryListItem(bool isMainCategory, string name, int id) : base()
			{
				this.isMainCategory = isMainCategory;
				this.name = name;
				this.id = id;
			}

			public override string ToString()
			{
				return "[isMainCategory=" + isMainCategory + ", name=" + name + ", id=" + id + "]";
			}
		}

		private static IList<POICategoryListItem> ListItems
		{
			get
			{
				IList<POICategoryListItem> listItems = new List<POICategoryListItem>();
				foreach (SKCategories.SKPOIMainCategory mainCategory in SKCategories.SKPOIMainCategory.Values())
				{
					listItems.Add(new POICategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1));
					foreach (int categoryId in SKUtils.GetSubcategoriesForCategory(mainCategory.Value))
					{
						listItems.Add(new POICategoryListItem(false, SKUtils.GetMainCategoryForCategory(categoryId).Names[0].ToUpper().Replace("_", " "), categoryId));
					}
				}
				return listItems;
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_list);

			FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

			listItems = ListItems;

			listView = (ListView) FindViewById(Resource.Id.list_view);
			listView.Visibility = ViewStates.Visible;

			adapter = new POICategoryListAdapter(this);
			listView.Adapter = adapter;

			Toast.MakeText(this, "Select the desired POI categories for heat map display", ToastLength.Short).Show();

			listView.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly POICategoriesListActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(POICategoriesListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
			{
				POICategoryListItem selectedItem = outerInstance.listItems[position];
				if (selectedItem.id > 0)
				{
					if (outerInstance.selectedCategories.Contains(selectedItem.id))
					{
						outerInstance.selectedCategories.RemoveAt(Convert.ToInt32(selectedItem.id));
						view.SetBackgroundColor(Resources.GetColor(Resource.Color.white));
					}
					else
					{
						outerInstance.selectedCategories.Add(selectedItem.id);
						view.SetBackgroundColor(Resources.GetColor(Resource.Color.selected));
					}

					Button showButton = (Button) FindViewById(Resource.Id.show_heat_map);
					if (outerInstance.selectedCategories.Count == 0)
					{
						showButton.Visibility = ViewStates.Gone;
					}
					else
					{
						showButton.Visibility = ViewStates.Visible;
					}
				}
			}
		}

        [Export("OnClick")]
		public virtual void onClick(View v)
		{
			if (v.Id == Resource.Id.show_heat_map)
			{
				SKCategories.SKPOICategory[] categories = new SKCategories.SKPOICategory[selectedCategories.Count];
				for (int i = 0; i < selectedCategories.Count; i++)
				{
					categories[i] = SKCategories.SKPOICategory.ForInt(selectedCategories[i].Value);
				}
				MapActivity.heatMapCategories = categories;
				Finish();
			}
		}

		private class POICategoryListAdapter : BaseAdapter
		{
			private readonly POICategoriesListActivity outerInstance;

			public POICategoryListAdapter(POICategoriesListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return outerInstance.listItems.Count;
				}
			}

			public override object getItem(int position)
			{
				return outerInstance.listItems[position];
			}

			public override long getItemId(int position)
			{
				return 0;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				TextView view = null;
				if (convertView == null)
				{
					view = new TextView(outerInstance);
				}
				else
				{
					view = (TextView) convertView;
				}

				POICategoryListItem item = outerInstance.listItems[position];

				view.Text = "  " + item.name;
				if (item.isMainCategory)
				{
					view.SetTextAppearance(outerInstance, Resource.Style.menu_options_group_style);
					view.SetBackgroundColor(Resources.GetColor(Resource.Color.grey_options_group));
				}
				else
				{
					view.SetTextAppearance(outerInstance, Resource.Style.menu_options_style);
					if (!outerInstance.selectedCategories.Contains(item.id))
					{
						view.SetBackgroundColor(Resources.GetColor(Resource.Color.white));
					}
					else
					{
						view.SetBackgroundColor(Resources.GetColor(Resource.Color.selected));
					}
				}
				return view;
			}
		}
	}

}