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

namespace Ninput
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public uint SelectedModifier { get; private set; }
        public uint SelectedKey { get; private set; }
        public string? KeyName { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            cbModifier.Items.Add("ALT");
            cbModifier.Items.Add("CTRL");
            cbModifier.SelectedIndex = 0;

            cbKey.Items.Add("K");
            cbKey.Items.Add("L");
            cbKey.Items.Add("F1");
            cbKey.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SelectedModifier = (cbModifier.Text == "ALT") ? 0x0001u : 0x0002u;

            if (cbKey.Text == "K") SelectedKey = 0x4Bu;
            else if (cbKey.Text == "L") SelectedKey = 0x4Cu;
            else if (cbKey.Text == "F1") SelectedKey = 0x70u;

            KeyName = $"{cbModifier.Text} + {cbKey.Text}";

            Properties.Settings.Default.Modifier = SelectedModifier;
            Properties.Settings.Default.Key = SelectedKey;
            Properties.Settings.Default.KeyName = KeyName;
            Properties.Settings.Default.Save();

            this.DialogResult = true;
        }
    }
}
