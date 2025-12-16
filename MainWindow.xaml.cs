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
        private long? _editingAnswerId = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCategories_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = _dbHelper.GetCategories();
            CategoriesGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadAnswers_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = _dbHelper.GetAnswers();
            AnswersGrid.ItemsSource = dt.DefaultView;
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

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            string answerText = AnswerTextBox.Text;
            string questionIdText = QuestionIdTextBox.Text;
            bool correctAnswer = CorrectAnswerCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(answerText) || string.IsNullOrWhiteSpace(questionIdText))
            {
                MessageBox.Show("Bitte alle Felder ausfüllen!");
                return;
            }

            if (!long.TryParse(questionIdText, out long questionId))
            {
                MessageBox.Show("Ungültige Fragen-ID!");
                return;
            }

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                
                if (_editingAnswerId.HasValue)
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "UPDATE answer SET question = @question, text = @text, correct_answer = @correct_answer WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@question", questionId);
                    cmd.Parameters.AddWithValue("@text", answerText);
                    cmd.Parameters.AddWithValue("@correct_answer", correctAnswer ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", _editingAnswerId.Value);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Antwort aktualisiert!");
                    _editingAnswerId = null;
                    
                    Button btn = (Button)sender;
                    btn.Content = "Antwort hinzufügen";
                }
                else
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO answer (question, text, correct_answer, answered_correctly, answered_incorrectly) VALUES (@question, @text, @correct_answer, 0, 0)", conn);
                    cmd.Parameters.AddWithValue("@question", questionId);
                    cmd.Parameters.AddWithValue("@text", answerText);
                    cmd.Parameters.AddWithValue("@correct_answer", correctAnswer ? 1 : 0);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Antwort hinzugefügt!");
                }
            }

            AnswerTextBox.Text = "";
            QuestionIdTextBox.Text = "";
            CorrectAnswerCheckBox.IsChecked = false;
            LoadAnswers_Click(null, null);
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen!");
                return;
            }

            DataRowView row = (DataRowView)CategoriesGrid.SelectedItem;
            long categoryId = Convert.ToInt64(row["id"]);

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

        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Antwort auswählen!");
                return;
            }

            DataRowView row = (DataRowView)AnswersGrid.SelectedItem;
            long answerId = Convert.ToInt64(row["id"]);

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    "DELETE FROM answer WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", answerId);
                cmd.ExecuteNonQuery();
            }

            LoadAnswers_Click(null, null);
            MessageBox.Show("Antwort gelöscht!");
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Kategorie auswählen!");
                return;
            }

            DataRowView row = (DataRowView)CategoriesGrid.SelectedItem;
            long categoryId = Convert.ToInt64(row["id"]);
            string oldName = (string)row["name"];

            _editingCategoryId = categoryId;
            CategoryNameTextBox.Text = oldName;
            CategoryNameTextBox.Focus();
            CategoryNameTextBox.SelectAll();

            Button addButton = (Button)((StackPanel)CategoryNameTextBox.Parent).Children[1];
            addButton.Content = "Speichern";
        }

        private void EditAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Antwort auswählen!");
                return;
            }

            DataRowView row = (DataRowView)AnswersGrid.SelectedItem;
            long answerId = Convert.ToInt64(row["id"]);
            long questionId = Convert.ToInt64(row["question"]);
            string text = (string)row["text"];
            long correctAnswer = Convert.ToInt64(row["correct_answer"]);

            _editingAnswerId = answerId;
            QuestionIdTextBox.Text = questionId.ToString();
            AnswerTextBox.Text = text;
            CorrectAnswerCheckBox.IsChecked = correctAnswer == 1;
            AnswerTextBox.Focus();
            AnswerTextBox.SelectAll();

            AddAnswerButton.Content = "Speichern";
        }
    }
}
