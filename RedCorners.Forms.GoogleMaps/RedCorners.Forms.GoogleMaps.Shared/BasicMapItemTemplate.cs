using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public class BasicMapItemTemplate : DataTemplateSelector
    {

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return new DataTemplate(new Func<object>(() => item));
        }
    }
}
