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
    /// <summary>
    /// Interaction logic for AddCategoryDialogWindow.xaml
    /// </summary>
    public partial class AddCategoryDialogWindow : Window
    {
        private readonly ImageMetadata _metadata;
        private readonly CategoriesManager _categoriesManager;
        public AddCategoryDialogWindow(ImageMetadata metadata, CategoriesManager categoriesManager)
        {
            InitializeComponent();
            this._metadata = metadata;
            this._categoriesManager = categoriesManager;
        }

        private void addCategory(object sender, RoutedEventArgs e)
        {
            if (CategoryNameTextBox.Text != "")
            {
                if (!_metadata.addCategory(new Category(CategoryNameTextBox.Text)))
                {
                    ToolTip tooltip = new ToolTip() { Content = "Kategoria już istnieje!" };
                    AddCategoryButton.ToolTip = tooltip;
                    tooltip.IsOpen = true;
                }
            }

            _categoriesManager.RefreshList();
        }
    }
}
