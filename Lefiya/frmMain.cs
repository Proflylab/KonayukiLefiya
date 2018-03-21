using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Text.RegularExpressions;

using MetroFramework;
using MetroFramework.Forms;
using System.Reflection;

namespace Lefiya
{
    public partial class frmMain : MetroForm
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblVersion.Text = $"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        }
        private void ImportFiles(string[] files)
        {
            foreach (var file in files)
            {
                if (new string[] { ".ass", ".ssa" }.Contains(Path.GetExtension(file).ToLower()))
                    lstData.Items.Add(new ListViewItem(new string[] { Path.GetFileName(file), Path.GetDirectoryName(file) }));
            }
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            var ofdImport = new OpenFileDialog
            {
                Title = "IMPORT",
                Filter = "Advanced SubStation Alpha (*.ass,*.ssa)|*.ass;*.ssa",
                Multiselect = true
            };

            if (ofdImport.ShowDialog(this) == DialogResult.OK)
            {
                ImportFiles(ofdImport.FileNames);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (lstData.Items.Count > 0)
            {
                var fbdExport = new FolderBrowserDialog
                {
                    Description = "Browse folder to Export your Files."
                };

                if (fbdExport.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (ListViewItem file in lstData.Items)
                    {
                        var path = Path.Combine(file.SubItems[1].Text, file.SubItems[0].Text);

                        if (File.Exists(path))
                        {
                            var lines = File.ReadAllLines(path, Encoding.UTF8);
                            for (int line = 0; line < lines.Length; line++)
                            {
                                var idx = Regex.Match(lines[line], "Dialogue:(?<Layer>.*?),(?<Start>.*?),(?<End>.*?),(?<Style>.*?),(?<Name>.*?),(?<MarginL>.*?),(?<MarginR>.*?),(?<MarginV>.*?),(?<Effect>.*?),(?<Text>.*)").Groups["Text"].Index;
                                var len = Regex.Match(lines[line], "Dialogue:(?<Layer>.*?),(?<Start>.*?),(?<End>.*?),(?<Style>.*?),(?<Name>.*?),(?<MarginL>.*?),(?<MarginR>.*?),(?<MarginV>.*?),(?<Effect>.*?),(?<Text>.*)").Groups["Text"].Value.Length;
                                if (lines[line].Contains("Dialogue:"))
                                {
                                    if (lines[line].Contains("{"))
                                    {
                                        if (lines[line][lines[line].Length - 1] != '}')
                                        {
                                            lines[line] = lines[line] + '}';
                                        }
                                        for (int j = lines[line].Length - 1; j > -1; j--)
                                        {
                                            if (lines[line][j] == '{')
                                            {
                                                if (lines[line][j - 1] != '}' && j - 1 > idx)
                                                {
                                                    lines[line] = lines[line].Substring(0, j) + '}' + lines[line].Substring(j);
                                                }
                                            }
                                            if (lines[line][j] == '}')
                                            {
                                                if (j + 1 < lines[line].Length && lines[line][j + 1] != '{')
                                                {
                                                    lines[line] = lines[line].Substring(0, j + 1) + '{' + lines[line].Substring(j + 1);
                                                }
                                            }
                                        }
                                        if (lines[line][idx] != '{')
                                        {
                                            lines[line] = lines[line].Substring(0, idx) + "{" + lines[line].Substring(idx);
                                        }
                                    }
                                    else
                                    {
                                        if (len != 0)
                                        {
                                            lines[line] = lines[line].Substring(0, idx) + "{" + lines[line].Substring(idx) + "}";
                                        }
                                    }
                                }
                            }
                            File.WriteAllLines(Path.Combine(fbdExport.SelectedPath, Path.GetFileNameWithoutExtension(file.SubItems[0].Text)) + "_CLEAN.ass", lines, Encoding.UTF8);
                            MetroMessageBox.Show(this, "Complete", "MsgBox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstData.Items.Clear();
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            ImportFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
}