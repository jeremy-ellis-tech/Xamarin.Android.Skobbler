using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activity
{
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
				foreach (SKPOIMainCategory mainCategory in SKPOIMainCategory.values())
				{
					listItems.Add(new POICategoryListItem(true, mainCategory.ToString().Replace("SKPOI_MAIN_CATEGORY_", ""), -1));
					foreach (int categoryId in SKUtils.getSubcategoriesForCategory(mainCategory.Value))
					{
						listItems.Add(new POICategoryListItem(false, SKUtils.getMainCategoryForCategory(categoryId).Names[0].ToUpper().Replace("_", " "), categoryId));
					}
				}
				return listItems;
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_list;

			findViewById(R.id.label_operation_in_progress).Visibility = View.GONE;

			listItems = ListItems;

			listView = (ListView) findViewById(R.id.list_view);
			listView.Visibility = View.VISIBLE;

			adapter = new POICategoryListAdapter(this);
			listView.Adapter = adapter;

			Toast.makeText(this, "Select the desired POI categories for heat map display", Toast.LENGTH_SHORT).show();

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
						view.BackgroundColor = Resources.getColor(R.color.white);
					}
					else
					{
						outerInstance.selectedCategories.Add(selectedItem.id);
						view.BackgroundColor = Resources.getColor(R.color.selected);
					}

					Button showButton = (Button) findViewById(R.id.show_heat_map);
					if (outerInstance.selectedCategories.Count == 0)
					{
						showButton.Visibility = View.GONE;
					}
					else
					{
						showButton.Visibility = View.VISIBLE;
					}
				}
			}
		}

		public virtual void onClick(View v)
		{
			if (v.Id == R.id.show_heat_map)
			{
				SKPOICategory[] categories = new SKPOICategory[selectedCategories.Count];
				for (int i = 0; i < selectedCategories.Count; i++)
				{
					categories[i] = SKPOICategory.forInt(selectedCategories[i]);
				}
				MapActivity.heatMapCategories = categories;
				finish();
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
					view.setTextAppearance(outerInstance, R.style.menu_options_group_style);
					view.BackgroundColor = Resources.getColor(R.color.grey_options_group);
				}
				else
				{
					view.setTextAppearance(outerInstance, R.style.menu_options_style);
					if (!outerInstance.selectedCategories.Contains(item.id))
					{
						view.BackgroundColor = Resources.getColor(R.color.white);
					}
					else
					{
						view.BackgroundColor = Resources.getColor(R.color.selected);
					}
				}
				return view;
			}
		}
	}

}