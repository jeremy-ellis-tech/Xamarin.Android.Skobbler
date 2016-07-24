using Android.Support.V7.Widget;

namespace Skobbler.Ngx.SDKTools.Onebox.Adapters
{
    public partial class SKSearchResultAdapter : global::Android.Support.V7.Widget.RecyclerView.Adapter
    {
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            _OnBindViewHolder(holder as ResultsHolder, position);
        }
    }
}