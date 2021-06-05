using System;
using System.Collections.Generic;
using System.IO;
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

namespace RawConverter
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            // set labels
            LabelApplicationName.Content = AboutThisApp.name + " by Ludwig Sembach";
            LabelApplicationVersion.Content = "Application version V" + AboutThisApp.version;
            LabelApplicationBuildDate.Content = "Build date: " + AboutThisApp.buildDate;

            // set textboxes and label
            TextBoxWhatsNew.Text = File.ReadAllText("Resources/WhatsNew.txt");
            LabelApplicationDescription.Text = File.ReadAllText("Resources/ApplicationDescription.txt");
            TextBoxLibraries.Text = File.ReadAllText("Resources/Libraries.txt");
            TextBoxLicense.Text = File.ReadAllText("Resources/License.txt");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
