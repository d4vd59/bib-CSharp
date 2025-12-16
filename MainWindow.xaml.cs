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
using System.IO;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Quizduell
{
    public partial class MainWindow : Window
    {
        private Databasehelper _dbHelper = new Databasehelper();
        private long? _editingCategoryId = null;
        private long? _editingAnswerId = null;
        private long? _editingQuestionId = null;
        
        private byte[] _selectedMediaBytes = null;
        private string _selectedMimeType = null;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void SelectMedia_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Mediendatei auswählen";
            openFileDialog.Filter = "Alle Medien|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.mp4;*.avi;*.mov;*.webm|" +
                                   "Bilder|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                                   "Videos|*.mp4;*.avi;*.mov;*.webm|" +
                                   "Alle Dateien|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    
                    _selectedMediaBytes = File.ReadAllBytes(openFileDialog.FileName);
                      
                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    _selectedMimeType = GetMimeType(extension);
                                 
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    SelectedMediaLabel.Text = $"{fileName} ({_selectedMimeType})";
                    SelectedMediaLabel.Foreground = Brushes.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Laden der Datei: {ex.Message}");
                    _selectedMediaBytes = null;
                    _selectedMimeType = null;
                    SelectedMediaLabel.Text = "Fehler beim Laden";
                    SelectedMediaLabel.Foreground = Brushes.Red;
                }
            }
        }
        
        private string GetMimeType(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".svg":
                    return "image/svg+xml";
                
                case ".mp4":
                    return "video/mp4";
                case ".avi":
                    return "video/x-msvideo";
                case ".mov":
                    return "video/quicktime";
                case ".webm":
                    return "video/webm";
                case ".mkv":
                    return "video/x-matroska";
                
                case ".mp3":
                    return "audio/mpeg";
                case ".wav":
                    return "audio/wav";
                case ".ogg":
                    return "audio/ogg";
                
                default:
                    return "application/octet-stream";
            }
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

        private void LoadQuestions_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = _dbHelper.GetQuestions();
            QuestionsGrid.ItemsSource = dt.DefaultView;
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
                    
                    AddAnswerButton.Content = "Antwort hinzufügen";
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

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            string questionText = QuestionTextTextBox.Text;
            string categoryIdText = QuestionCategoryIdTextBox.Text;
            string difficultyText = QuestionDifficultyTextBox.Text;

            if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(categoryIdText))
            {
                MessageBox.Show("Bitte Text und Kategorie-ID ausfüllen!");
                return;
            }

            if (!long.TryParse(categoryIdText, out long categoryId))
            {
                MessageBox.Show("Ungültige Kategorie-ID!");
                return;
            }

            long difficulty = 0;
            if (!string.IsNullOrWhiteSpace(difficultyText) && !long.TryParse(difficultyText, out difficulty))
            {
                MessageBox.Show("Ungültige Schwierigkeit!");
                return;
            }

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                
                if (_editingQuestionId.HasValue)
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "UPDATE question SET text = @text, media = @media, mime_type = @mime_type, category = @category, difficulty = @difficulty WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@text", questionText);
                    cmd.Parameters.AddWithValue("@media", _selectedMediaBytes != null ? (object)_selectedMediaBytes : DBNull.Value);
                    cmd.Parameters.AddWithValue("@mime_type", _selectedMimeType != null ? (object)_selectedMimeType : DBNull.Value);
                    cmd.Parameters.AddWithValue("@category", categoryId);
                    cmd.Parameters.AddWithValue("@difficulty", difficulty);
                    cmd.Parameters.AddWithValue("@id", _editingQuestionId.Value);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Frage aktualisiert!");
                    _editingQuestionId = null;
                    AddQuestionButton.Content = "Frage hinzufügen";
                }
                else
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO question (text, media, mime_type, category, difficulty, answered_correctly, answered_incorrectly) VALUES (@text, @media, @mime_type, @category, @difficulty, 0, 0)", conn);
                    cmd.Parameters.AddWithValue("@text", questionText);
                    cmd.Parameters.AddWithValue("@media", _selectedMediaBytes != null ? (object)_selectedMediaBytes : DBNull.Value);
                    cmd.Parameters.AddWithValue("@mime_type", _selectedMimeType != null ? (object)_selectedMimeType : DBNull.Value);
                    cmd.Parameters.AddWithValue("@category", categoryId);
                    cmd.Parameters.AddWithValue("@difficulty", difficulty);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Frage hinzugefügt!");
                }
            }

            QuestionTextTextBox.Text = "";
            QuestionCategoryIdTextBox.Text = "";
            QuestionDifficultyTextBox.Text = "";
            _selectedMediaBytes = null;
            _selectedMimeType = null;
            SelectedMediaLabel.Text = "Keine Datei ausgewählt";
            SelectedMediaLabel.Foreground = Brushes.Gray;
            
            LoadQuestions_Click(null, null);
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

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Frage auswählen!");
                return;
            }

            DataRowView row = (DataRowView)QuestionsGrid.SelectedItem;
            long questionId = Convert.ToInt64(row["id"]);

            using (SQLiteConnection conn = _dbHelper.GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    "DELETE FROM question WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", questionId);
                cmd.ExecuteNonQuery();
            }

            LoadQuestions_Click(null, null);
            MessageBox.Show("Frage gelöscht!");
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

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsGrid.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Frage auswählen!");
                return;
            }

            DataRowView row = (DataRowView)QuestionsGrid.SelectedItem;
            long questionId = Convert.ToInt64(row["id"]);
            string text = (string)row["text"];
            
            
            if (row["media"] != DBNull.Value)
            {
                _selectedMediaBytes = (byte[])row["media"];
                _selectedMimeType = row["mime_type"] != DBNull.Value ? (string)row["mime_type"] : null;
                SelectedMediaLabel.Text = $"Medien vorhanden ({_selectedMimeType ?? "unbekannt"})";
                SelectedMediaLabel.Foreground = Brushes.Blue;
            }
            else
            {
                _selectedMediaBytes = null;
                _selectedMimeType = null;
                SelectedMediaLabel.Text = "Keine Datei ausgewählt";
                SelectedMediaLabel.Foreground = Brushes.Gray;
            }
            
            long categoryId = Convert.ToInt64(row["category"]);
            long difficulty = row["difficulty"] != DBNull.Value ? Convert.ToInt64(row["difficulty"]) : 0;

            _editingQuestionId = questionId;
            QuestionTextTextBox.Text = text;
            QuestionCategoryIdTextBox.Text = categoryId.ToString();
            QuestionDifficultyTextBox.Text = difficulty.ToString();
            QuestionTextTextBox.Focus();
            QuestionTextTextBox.SelectAll();

            AddQuestionButton.Content = "Speichern";
        }
    }
}
