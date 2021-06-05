using System.IO;
using System.Windows;

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

            // set title
            Title = AboutThisApp.name;

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
