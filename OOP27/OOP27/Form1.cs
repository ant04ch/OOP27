using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OOP27
{
    public partial class Form1 : Form
    {
        private string currentDirectory;

        public Form1()
        {
            InitializeComponent();
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;

            if (selectedNode != null)
            {
                string selectedPath = selectedNode.FullPath;

                if (Directory.Exists(selectedPath))
                {
                    currentDirectory = selectedPath;
                    LoadDirectories(selectedPath, selectedNode);
                    LoadFiles(selectedPath);
                }
            }
            if (selectedNode != null)
            {
                string selectedPath = selectedNode.FullPath;
                textBox1.Text = selectedPath; // Оновлення значення textBox1 з вибраним шляхом
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string selectedFileName = selectedItem.Text;
                string selectedFilePath = Path.Combine(currentDirectory, selectedFileName);

                if (File.Exists(selectedFilePath))
                {
                    FileInfo fileInfo = new FileInfo(selectedFilePath);
                    ShowFileProperties(fileInfo);
                }
            }
        }

        private void LoadDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                TreeNode driveNode = new TreeNode(drive.Name);
                driveNode.ImageIndex = 0;

                treeView1.Nodes.Add(driveNode);
            }
        }

        private void LoadDirectories(string path, TreeNode parentNode)
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                DirectoryInfo directory = new DirectoryInfo(path);

                // Очищення попередніх підтек та файлів
                parentNode.Nodes.Clear();
                listView1.Items.Clear();
                imageList1.Images.Clear();

                // Додавання батьківської теки до ListView
                if (parentNode.Parent != null)
                {
                    ListViewItem parentItem = new ListViewItem("..");
                    parentItem.ImageIndex = 1; // Індекс іконки теки
                    listView1.Items.Add(parentItem);
                }

                int imageIndex = 2; // Початковий індекс для іконок файлів

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    TreeNode directoryNode = new TreeNode(subDirectory.Name);
                    directoryNode.ImageIndex = 1; // Індекс іконки теки

                    parentNode.Nodes.Add(directoryNode);

                    // Додавання теки до ListView
                    ListViewItem item = new ListViewItem(subDirectory.Name);
                    item.ImageIndex = 1; // Індекс іконки теки у списку зображень

                    listView1.Items.Add(item);
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    Icon fileIcon = Icon.ExtractAssociatedIcon(file.FullName);
                    imageList1.Images.Add(fileIcon);

                    // Додавання файлу до ListView з відповідною іконкою
                    ListViewItem item = new ListViewItem(file.Name);
                    item.SubItems.Add(file.Length.ToString());
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    item.ImageIndex = imageIndex; // Встановлення індексу зображення

                    listView1.Items.Add(item);

                    imageIndex++; // Інкремент індексу зображення для наступного файлу
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadFiles(string path)
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                listView1.Items.Clear();
                imageList1.Images.Clear();

                int imageIndex = 0;

                foreach (FileInfo file in directory.GetFiles())
                {
                    Icon fileIcon = Icon.ExtractAssociatedIcon(file.FullName);
                    imageList1.Images.Add(fileIcon);
                    ListViewItem item = new ListViewItem(file.Name);
                    item.SubItems.Add(FormatSize(file.Length));
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    item.ImageIndex = imageIndex; 

                    listView1.Items.Add(item);

                    imageIndex++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading files: " + ex.Message);
            }
        }

        private void ShowFileProperties(FileInfo fileInfo)
        {
            label1.Text = fileInfo.Name;
            label2.Text = "Створено: " + fileInfo.CreationTime.ToString();
            label3.Text = "Оновлено: " + fileInfo.LastWriteTime.ToString();
            label4.Text = "Розмір: " + FormatSize(fileInfo.Length);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            treeView1.AfterSelect += treeView1_AfterSelect;
            LoadDrives();

            listView1.LargeImageList = imageList1;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;

            if (Directory.Exists(path))
            {
                currentDirectory = path;
                LoadDirectories(path, null); 
                LoadFiles(path);
            }
            else
            {
                MessageBox.Show("Вказано недійсний шлях до папки.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox2.Text;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                List<string> matchingFiles = SearchFiles(currentDirectory, searchTerm);

                if (matchingFiles.Count > 0)
                {
                    // Очищаємо ListView та imageList1
                    listView1.Items.Clear();
                    imageList1.Images.Clear();

                    foreach (string file in matchingFiles)
                    {
                        // Додаємо знайдений файл до ListView
                        ListViewItem item = new ListViewItem(file);
                        item.SubItems.Add(""); // Порожнє значення для додаткових стовпців (розмір, дата)

                        // Отримуємо іконку файлу
                        Icon fileIcon = Icon.ExtractAssociatedIcon(file);
                        imageList1.Images.Add(fileIcon);

                        // Встановлюємо індекс зображення для елементу
                        item.ImageIndex = imageList1.Images.Count - 1;

                        listView1.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("Файли зі словом \"" + searchTerm + "\" не знайдені.");
                }
            }
            else
            {
                MessageBox.Show("Введіть слово для пошуку.");
            }
        }
        private List<string> SearchFiles(string path, string searchTerm)
        {
            List<string> matchingFiles = new List<string>();

            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                foreach (FileInfo file in directory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Name.Contains(searchTerm))
                    {
                        matchingFiles.Add(file.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час пошуку файлів: " + ex.Message);
            }

            return matchingFiles;
        }

        private string FormatSize(long size)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            string sizeText;

            if (size >= GB)
            {
                double sizeInGB = (double)size / GB;
                sizeText = string.Format("{0:0.00} ГБ", sizeInGB);
            }
            else if (size >= MB)
            {
                double sizeInMB = (double)size / MB;
                sizeText = string.Format("{0:0.00} МБ", sizeInMB);
            }
            else if (size >= KB)
            {
                double sizeInKB = (double)size / KB;
                sizeText = string.Format("{0:0.00} КБ", sizeInKB);
            }
            else
            {
                sizeText = size.ToString() + " байт";
            }

            return sizeText;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Parent != null)
            {
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[0];
            }
        }
    }
}
