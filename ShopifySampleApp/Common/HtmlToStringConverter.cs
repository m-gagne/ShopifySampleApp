using System;
using System.Net;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Data;

namespace ShopifySampleApp.Common
{
    /// <summary>
    /// Value converter that translates HTML to a string
    /// </summary>
    public sealed class HtmlToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var html = (string)value;

            // create whitespace between html elements, so that words do not run together
            html = html.Replace(">", "> ");

            // parse html
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // strip html decoded text from html
            string text = WebUtility.HtmlDecode(doc.DocumentNode.InnerText);

            // replace all whitespace with a single space and remove leading and trailing whitespace
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
