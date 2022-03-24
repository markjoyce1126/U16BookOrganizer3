﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Text.Json;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Security.Cryptography;
using WinForms = System.Windows.Forms;

namespace WPFBookOrganizer


{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static class filepicker
        {
            public static string filename = "csvstuff.csv";
        }

        public class tmpcreator
        {
            public static void Main2()
            {
                string path = @"c:\tmp";

                try
                {
                    if (Directory.Exists(path))
                    {
                        return;
                    }

                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);

                }
                catch (Exception e)
                {

                }
                finally { }
            }
        }

        public class inputcreator
        {
            public static void Main3() {
                var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".csv";
            dialog.Filter = "BookData Insertion (.csv)|*.csv";

            bool? result = dialog.ShowDialog();

            filepicker.filename = dialog.FileName;
        }
    }

        public class outputcreator
        {
            public static void Main4()
            {
                WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
                folderDialog.ShowNewFolderButton = false;
                WinForms.DialogResult result = folderDialog.ShowDialog();
                MessageBox.Show($"You have selected {folderDialog.SelectedPath} for output");
            }
        }
    

        private void Button_Click_Input(object sender, RoutedEventArgs e)
        {
            inputcreator.Main3();
            MessageBox.Show($"You have selected {filepicker.filename} for input. You'll find the output in the tmp folder in the C drive.");
        }


        private void Button_Click_Output(object sender, RoutedEventArgs e) // Works and allows folder selection, but is not currently relevant to the outcome of the program (it'll just go to the tmp folder on the C drive)
        {
            outputcreator.Main4();
            MessageBox.Show($"Output folder chosen successfully");
        }

        private void Button_Click_Tmp_Create(object sender, RoutedEventArgs e) // Is a stand in for the Output atm, so that the user can still access the completed file.
        {
            tmpcreator.Main2();
            MessageBox.Show("tmp folder successfully created, it's at c:/tmp");
        }

        private void Button_Click_Go(object sender, RoutedEventArgs e)
        {
            BookOrganizer.BookOrganizerVital();
        }


        public class BookOrganizer
        {
            public static void BookOrganizerVital()
            {
                string csv_file_path = filepicker.filename;
                DataTable csvData = CSVreader.GetDataTableFromCSVFile(csv_file_path);
                MessageBox.Show($"Read {csvData.Rows.Count} records");
                List<Book> Books = new List<Book>();
                foreach (DataRow row in csvData.Rows)
                {
                    Books.Add(new Book(row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString()));
                }
                Console.WriteLine($"Sample - Books 50 was {Books[50]} ");

                SerializeToFile(Books, @"c:/tmp/output.json");
            }

            public static async Task SerializeToFile(object data, string filename)

            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                using var stream = File.Create(filename);
                await JsonSerializer.SerializeAsync(stream, data, options);
                await stream.DisposeAsync();
            }
        }

        static class CSVreader
        {
            public static DataTable GetDataTableFromCSVFile(string csv_file_path)
            {
                DataTable csvData = new DataTable();
                try
                {
                    using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                    {
                        csvReader.SetDelimiters(new string[] { "," });
                        csvReader.HasFieldsEnclosedInQuotes = true;
                        string[] colFields = csvReader.ReadFields();

                        foreach (string column in colFields)
                        {
                            DataColumn datecolumn = new DataColumn(column);
                            datecolumn.AllowDBNull = true;
                            csvData.Columns.Add(datecolumn);
                        }

                        while (!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();
                            //Making empty value as null
                            for (int i = 0; i < fieldData.Length; i++)
                            {
                                if (fieldData[i] == "")
                                {
                                    fieldData[i] = null;
                                }
                          }

                            csvData.Rows.Add(fieldData);
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                return csvData;
            }
        }

        public readonly record struct Book
        {

            public Book(string name, string title, string publishedIn, string publisher, string date)
            {

                Name = name;
                Title = title;
                PublishedIn = publishedIn;
                Publisher = publisher;
                Date = date;
                Cat = GetCatFor(name, title, publishedIn, publisher, date);
            }

            public string Cat { get; }
            public string Name { get; }
            public string Title { get; }
            public string PublishedIn { get; }
            public string Publisher { get; }
            public string Date { get; }


            private bool PrintMembers(StringBuilder stringBuilder)
            {
                stringBuilder.Append($"Catalogue = {Cat}, Author = {Name}, Title = {Title}, Published in {PublishedIn} by {Publisher}, {Date}");
                return true;
            }

            private static string GetCatFor(string name, string title, string publishedIn, string publisher, string date)
            {
                string source = name + title + publishedIn + publisher + date;
                using (MD5 md5 = MD5.Create())
                {
                    string hash = GetHash(md5, source);
                    return hash[..10];
                }
            }

            private static string GetHash(HashAlgorithm hashAlgorithm, string input)
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }

        }
    }
}