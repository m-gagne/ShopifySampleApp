using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using ShopifyAPIAdapterLibrary;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace ShopifySampleApp.Data
{
    /// <summary>
    /// Base class for <see cref="ProductDataItem"/> and <see cref="ProductDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class ProductDataCommon : ShopifySampleApp.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public ProductDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(ProductDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class ProductDataItem : ProductDataCommon
    {
        public ProductDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, ProductDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private ProductDataGroup _group;
        public ProductDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ProductDataGroup : ProductDataCommon
    {
        public ProductDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<ProductDataItem> _items = new ObservableCollection<ProductDataItem>();
        public ObservableCollection<ProductDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<ProductDataItem> _topItem = new ObservableCollection<ProductDataItem>();
        public ObservableCollection<ProductDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// ProductDataSource initializes with placeholder data rather than live production
    /// data so that Product data is provided at both design-time and run-time.
    /// </summary>
    public sealed class ProductDataSource
    {
        private static ProductDataSource _ProductDataSource = new ProductDataSource();

        private ObservableCollection<ProductDataGroup> _allGroups = new ObservableCollection<ProductDataGroup>();
        public ObservableCollection<ProductDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<ProductDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _ProductDataSource.AllGroups;
        }

        public static ProductDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _ProductDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static ProductDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _ProductDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public ProductDataSource()
        {

            // TODO: Of course it's a bad idea to hardcode this. In fact having this in your app is a bad idea period. This really should be exposed via
            // your own custom REST API that then connected to Shopify's.
            // This is just an example to show that it can be done, not how it *should* be done.
            ShopifyAuthorizationState authState = new ShopifyAuthorizationState
            {
                ShopName = "buckridge-hyatt6895",
                AccessToken = "42d4917ed3507278c748046addb01f3d"
            };

            var api = new ShopifyAPIClient(authState, new JsonDataTranslator());

            // The JSON Data Translator will automatically decode the JSON for you
            var result = api.Get("/admin/products.json");
            
            result.Completed = delegate(IAsyncOperation<dynamic> asyncAction, AsyncStatus asyncStatus)
            {
                var data = asyncAction.GetResults();
                var products = data.products;

                foreach (var product in products)
                {
                    var group = new ProductDataGroup(product.id.ToString(),
                                        product.title.ToString(),
                                        product.vendor.ToString(),
                                        product.images.Count > 0 ? product.images[0].src.ToString() : "",
                                        product.body_html.ToString());

                    var imgCount = 0;
                    foreach (var variant in product.variants)
                    {
                        group.Items.Add(new ProductDataItem(variant.id.ToString(),
                                            variant.option1.ToString(),
                                            variant.option2.ToString(),
                                            imgCount < product.images.Count ? product.images[imgCount++].src.ToString() : "",
                                            "",
                                            group.Description,
                                            group));                            
                    }
                    this.AllGroups.Add(group);
                }
            };
        }
    }
}
