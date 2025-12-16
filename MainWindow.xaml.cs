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
using System.Data;

namespace Quizduell
{
    public partial class MainWindow : Window
    {
        private Databasehelper _dbHelper = new Databasehelper();
        private long? _editingCategoryId = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCategories_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = _dbHelper.GetCategories();
            CategoriesGrid.ItemsSource = dt.DefaultView;
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Bitte Namen eingeben!");
                return;
            }

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                
                if (_editingCategoryId.HasValue)
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "UPDATE category SET name = @name WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@name", categoryName);
                    cmd.Parameters.AddWithValue("@id", _editingCategoryId.Value);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Kategorie aktualisiert!");
                    _editingCategoryId = null;
                    
                    Button btn = (Button)sender;
                    btn.Content = "Kategorie hinzufügen";
                }
                else
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO category (name) VALUES (@name)", conn);
                    cmd.Parameters.AddWithValue("@name", categoryName);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Kategorie hinzugefügt!");
                }
            }

            CategoryNameTextBox.Text = "";
            LoadCategories_Click(null, null);
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen!");
                return;
            }

            DataRowView row = (DataRowView)CategoriesGrid.SelectedItem;
            long categoryId = (long)row["id"];

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    "DELETE FROM category WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", categoryId);
                cmd.ExecuteNonQuery();
            }

            LoadCategories_Click(null, null);
            MessageBox.Show("Kategorie gelöscht!");
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen!");
                return;
            }

            DataRowView row = (DataRowView)CategoriesGrid.SelectedItem;
            long categoryId = (long)row["id"];
            string oldName = (string)row["name"];

            _editingCategoryId = categoryId;
            CategoryNameTextBox.Text = oldName;
            CategoryNameTextBox.Focus();
            CategoryNameTextBox.SelectAll();

            Button addButton = (Button)((StackPanel)CategoryNameTextBox.Parent).Children[1];
            addButton.Content = "Speichern";
        }
    }
}
