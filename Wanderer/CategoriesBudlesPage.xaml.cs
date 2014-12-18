using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.Diagnostics;

namespace Wanderer
{
    public partial class CategoriesBudlesPage : PhoneApplicationPage
    {

        public List<String> _categories = new List<String>();

        public CategoriesBudlesPage()
        {

            InitializeComponent();
            CategoriesListBox.DataContext = _categories;
            DAO.SendRequestForCategories(this);

        }


        public void CategoriesRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);
                    string json = streamReader.ReadToEnd();

                    JSONParser parser = new JSONParser();
                    _categories.AddRange(parser.ParceCategoriesJSON(json));

                    CategoriesListBox.DataContext = null;
                    CategoriesListBox.DataContext = _categories;

                    Debug.WriteLine("---JSON, req : " + json);
                });
            }
        }
    }
}