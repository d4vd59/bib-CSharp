using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quizduell
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    using System.Data;

    public partial class MainWindow : Window
    {
        private Databasehelper _dbHelper = new Databasehelper();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCategories_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = _dbHelper.GetCategories();
            CategoriesGrid.ItemsSource = dt.DefaultView;
        }
    }
}
