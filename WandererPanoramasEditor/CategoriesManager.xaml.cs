using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WandererPanoramasEditor
{
    public partial class CategoriesManager : Window
    {
        #region Members
        private readonly ImageMetadata _metadata;
        #endregion

        #region Constructors
        public CategoriesManager(ImageMetadata metadata)
        {
            InitializeComponent();
            this._metadata = metadata;
            RefreshList();
        }
        #endregion

        #region Event Handlers
        private void AddCategory(object sender, RoutedEventArgs e)
        {
            AddCategoryDialogWindow addCategoryWindow = new AddCategoryDialogWindow(_metadata, this);
            addCategoryWindow.ShowDialog();
        }

        private void RemoveCategory(object sender, RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem != null) {
                if (!_metadata.RemoveCategory((Category)CategoryList.SelectedItem)) {
                    ToolTip tooltip = new ToolTip() { Content = "Kategoria jest w użyciu!" };
                    RemoveCategoryButton.ToolTip = tooltip;
                    tooltip.IsOpen = true;
                }
            }
            RefreshList();
        }
        #endregion

        #region Help Methods
        internal void RefreshList(){
            CategoryList.ItemsSource = null;
            CategoryList.ItemsSource = _metadata.Categories;
        }
        #endregion

    }
}
