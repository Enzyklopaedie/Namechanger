using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Namechanger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2_state();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2_state();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button2_state();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog
            {
                Description = "Wähle den Ordner des Projektes, das du umbennen willst",
                ShowNewFolderButton = false
            };

            DialogResult result = folder.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folder.SelectedPath))
            {
                textBox2.Text = folder.SelectedPath;
                button2_state();
            }
            else
            {
                MessageBox.Show("Kein Ordner ausgewählt.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string new_name = textBox1.Text;
            string path = textBox2.Text;

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                ShowErrorMessage("Bitte geben Sie einen gültigen Ordnerpfad ein oder wählen Sie einen Ordner aus.");
                return;
            }

            if (!valid_project_folder(path))
            {
                ShowErrorMessage("Der ausgewählte Ordner enthält keine gültige Projektstruktur. Stellen Sie sicher, dass eine .sln-Datei und ein gleichnamiger Unterordner vorhanden sind.");
                return;
            }

            if (Directory.Exists(Path.Combine(Path.GetDirectoryName(path), new_name)))
            {
                ShowErrorMessage("Ein Projekt mit dem neuen Namen existiert bereits in dem Verzeichnis.");
                return;
            }

            try
            {
                string old_name = Path.GetFileName(path);
                namechanger(path, old_name, new_name);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Fehler beim Umbenennen: {ex.Message}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            label3.Text = message;
            label3.ForeColor = Color.Red;
            label3.Visible = true;
        }

        private bool IsValidProjectName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName)) return false;
            return Regex.IsMatch(projectName, @"^[a-zA-Z0-9äöüÄÖÜß_\- ]+$");
        }

        private bool valid_project_folder(string path)
        {
            string sln_path = Directory.GetFiles(path, "*.sln").FirstOrDefault();
            if (sln_path == null) return false;

            string sln_name = Path.GetFileNameWithoutExtension(sln_path);
            bool matching_folder = Directory.GetDirectories(path).Any(d => Path.GetFileName(d).Equals(sln_name, StringComparison.OrdinalIgnoreCase));

            return matching_folder;
        }

        private void namechanger(string path, string old_name, string new_name)
        {
            delete(path, new[] { "obj", "bin" });
            change(path, old_name, new_name);
            rename(path, old_name, new_name);
            string new_path = Path.Combine(Path.GetDirectoryName(path), new_name);
            Directory.Move(path, new_path);
            label3.Text = $"Operation erfolgreich. Projekt '{old_name}' wurde in '{new_name}' umbenannt.";
            label3.ForeColor = Color.Green;
            label3.Visible = true;
        }

        private void delete(string rootPath, string[] directories_to_delete)
        {
            foreach (string dir in directories_to_delete)
            {
                string directory_path = Path.Combine(rootPath, dir);
                if (Directory.Exists(directory_path))
                {
                    Directory.Delete(directory_path, true);
                }
            }
        }

        private void change(string path, string old_name, string new_name)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                      .Where(file => !file.StartsWith(Path.Combine(path, ".vs"))).ToArray();

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                content = content.Replace(old_name, new_name);
                File.WriteAllText(file, content);
            }
        }

        private void rename(string path, string old_name, string new_name)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                      .Where(file => !file.StartsWith(Path.Combine(path, ".vs"))).ToArray();

            foreach (string file in files)
            {
                if (Path.GetFileName(file).Contains(old_name))
                {
                    string new_file_name = Path.GetFileName(file).Replace(old_name, new_name);
                    string new_file_path = Path.Combine(Path.GetDirectoryName(file), new_file_name);
                    File.Move(file, new_file_path);
                }
            }

            string[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Where(directory => !directory.StartsWith(Path.Combine(path, ".vs"))).ToArray();
            foreach (string directory in directories)
            {
                if (Path.GetFileName(directory).Contains(old_name))
                {
                    string new_directory_name = Path.GetFileName(directory).Replace(old_name, new_name);
                    string new_directory_path = Path.Combine(Path.GetDirectoryName(directory), new_directory_name);
                    Directory.Move(directory, new_directory_path);
                }
            }
        }

        private void button2_state()
        {
            string new_name = textBox1.Text;
            string path = textBox2.Text;
            button2.Enabled = IsValidProjectName(new_name) && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
    }
}
