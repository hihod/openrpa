﻿using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenRPA.IE
{
    public partial class GetElementDesigner : INotifyPropertyChanged
    {
        public GetElementDesigner()
        {
            InitializeComponent();
            HighlightImage = IEExtensions.GetImageSourceFromResource("search.png");
            Loaded += (sender, e) =>
            {
                var Variables = ModelItem.Properties[nameof(GetElement.Variables)].Collection;
                if (Variables != null && Variables.Count == 0)
                {
                    Variables.Add(new Variable<int>("Index", 0));
                    Variables.Add(new Variable<int>("Total", 0));
                }
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public BitmapFrame HighlightImage { get; set; }
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Open_Selector(object sender, RoutedEventArgs e)
        {
            ModelItem loadFrom = ModelItem.Parent;
            string loadFromSelectorString = "";
            IESelector anchor = null;
            while (loadFrom.Parent != null)
            {
                var p = loadFrom.Properties.Where(x => x.Name == "Selector").FirstOrDefault();
                if (p != null)
                {
                    loadFromSelectorString = loadFrom.GetValue<string>("Selector");
                    anchor = new IESelector(loadFromSelectorString);
                    break;
                }
                loadFrom = loadFrom.Parent;
            }
            string SelectorString = ModelItem.GetValue<string>("Selector");
            int maxresults = ModelItem.GetValue<int>("MaxResults");
            Interfaces.Selector.SelectorWindow selectors;
            if (!string.IsNullOrEmpty(SelectorString))
            {
                var selector = new IESelector(SelectorString);
                selectors = new Interfaces.Selector.SelectorWindow("IE", selector, anchor, maxresults);
            }
            else
            {
                var selector = new IESelector("[{Selector: 'IE'}]");
                selectors = new Interfaces.Selector.SelectorWindow("IE", selector, anchor, maxresults);
            }
            selectors.Owner = Interfaces.GenericTools.MainWindow;
            if (selectors.ShowDialog() == true)
            {
                ModelItem.Properties["Selector"].SetValue(new InArgument<string>() { Expression = new Literal<string>(selectors.vm.json) });
                var l = selectors.vm.Selector.Last();
                if (l.Element != null)
                {
                    ModelItem.Properties["Image"].SetValue(l.Element.ImageString());
                    NotifyPropertyChanged("Image");
                }
                if (anchor != null)
                {
                    ModelItem.Properties["From"].SetValue(new InArgument<IEElement>()
                    {
                        Expression = new Microsoft.VisualBasic.Activities.VisualBasicValue<IEElement>("item")
                    });
                    ModelItem.Properties["MinResults"].SetValue(new InArgument<int>()
                    {
                        Expression = new Microsoft.VisualBasic.Activities.VisualBasicValue<int>("0")
                    });
                    ModelItem.Properties["Timeout"].SetValue(new InArgument<TimeSpan>()
                    {
                        Expression = new Microsoft.VisualBasic.Activities.VisualBasicValue<TimeSpan>("TimeSpan.FromSeconds(0)")
                    });
                }
            }
        }
        private void Highlight_Click(object sender, RoutedEventArgs e)
        {
            ModelItem loadFrom = ModelItem.Parent;
            string loadFromSelectorString = "";
            IESelector anchor = null;
            while (loadFrom.Parent != null)
            {
                var p = loadFrom.Properties.Where(x => x.Name == "Selector").FirstOrDefault();
                if (p != null)
                {
                    loadFromSelectorString = loadFrom.GetValue<string>("Selector");
                    anchor = new IESelector(loadFromSelectorString);
                    break;
                }
                loadFrom = loadFrom.Parent;
            }

            HighlightImage = IEExtensions.GetImageSourceFromResource(".x.png");
            NotifyPropertyChanged("HighlightImage");

            string SelectorString = ModelItem.GetValue<string>("Selector");
            int maxresults = ModelItem.GetValue<int>("MaxResults");
            var selector = new IESelector(SelectorString);
            var elements = new List<IEElement>();
            if (anchor != null)
            {
                var _base = IESelector.GetElementsWithuiSelector(anchor, null, 10);
                foreach (var _e in _base)
                {
                    var res = IESelector.GetElementsWithuiSelector(selector, _e, maxresults);
                    elements.AddRange(res);
                }

            }
            else
            {
                var res = IESelector.GetElementsWithuiSelector(selector, null, maxresults);
                elements.AddRange(res);
            }

            if (elements.Count() > 0)
            {
                HighlightImage = IEExtensions.GetImageSourceFromResource("check.png");
                NotifyPropertyChanged("HighlightImage");
            }
            foreach (var ele in elements) ele.Highlight(false, System.Drawing.Color.Red, TimeSpan.FromSeconds(1));
        }
        public string ImageString
        {
            get
            {
                string result = string.Empty;
                result = ModelItem.GetValue<string>("Image");
                return result;
            }
        }
        public BitmapImage Image
        {
            get
            {
                var image = ImageString;
                System.Drawing.Bitmap b = Task.Run(() =>
                {
                    return Interfaces.Image.Util.LoadBitmap(image);
                }).Result;
                using (b)
                {
                    if (b == null) return null;
                    return Interfaces.Image.Util.BitmapToImageSource(b, Interfaces.Image.Util.ActivityPreviewImageWidth, Interfaces.Image.Util.ActivityPreviewImageHeight);
                }
            }
        }
    }
}