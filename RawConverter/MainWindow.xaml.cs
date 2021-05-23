using System;
using System.Windows;
using System.Windows.Controls;

namespace RawConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // static attributes for menu state
        private const int defaultMenuWidth = 70;
        private const int expandedMenuWidth = 250;
        static bool menuVisible = false;

        public MainWindow()
        {
            InitializeComponent();
            ResizeMenuColum(width: defaultMenuWidth);
            SetAppInfo();

            // function to set app info
            void SetAppInfo()
            {
                TextBlockInfo.Text = AboutThisApp.name + " V" + AboutThisApp.version + Environment.NewLine + "Bulid date: " + AboutThisApp.builtDate; 
            }
        }

        /// <summary>
        /// Method to resize the menu column. Elements will be set to visible/invisible while changing the column width.
        /// </summary>
        /// <param name="width"></param>
        private void ResizeMenuColum(int width)
        {
            // function to toffle the visibility of all menu controls
            // expanders wille be collapsed
            void ToggleMenuWidgets(bool state)
            {
                // initilite variables for this method
                Visibility visibilityState;

                // set visibility variable
                if (state == true)
                {
                    visibilityState = Visibility.Visible;
                }
                else
                {
                    visibilityState = Visibility.Hidden;
                }

                // set visibility properties
                Expander[] expanderControls = { ExpanderOutputFileType, ExpanderOutputFolder, ExpanderInputFileType };
                foreach (Expander control in expanderControls)
                {
                    control.Visibility = visibilityState;
                    control.IsExpanded = false;
                }
                ButtonMoreInfo.Visibility = visibilityState;
                TextBlockInfo.Visibility = visibilityState;
            }

            // toggle elements
            ToggleMenuWidgets(state: menuVisible);

            // resize
            GridLength newWidth = new(width, GridUnitType.Pixel);
            ColumnMenu.Width = newWidth;
        }

        /// <summary>
        /// Method to refresh the data grid with all selected files
        /// </summary>
        private void RefreshDataGrid()
        {
            // set source
            DataGridFiles.ItemsSource = RawFileReader.dataTableFiles.DefaultView;

            // set column widths
            DataGridLength[] columnWidths = { new DataGridLength(3, DataGridLengthUnitType.Star), new DataGridLength(1, DataGridLengthUnitType.Auto), new DataGridLength(1, DataGridLengthUnitType.Auto) };
            int counter = 0;
            foreach (DataGridColumn column in DataGridFiles.Columns)
            {
                column.Width = columnWidths[counter];
            }       
        }

        /// <summary>
        /// Event to trigger the Menu resize when the menu button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            // set the menuVisible variabel and resize the menu
            if (menuVisible == true)
            {
                menuVisible = false;
                ResizeMenuColum(width: defaultMenuWidth);
            }
            else
            {
                menuVisible = true;
                ResizeMenuColum(width: expandedMenuWidth);
            }
        }

        /// <summary>
        /// Event to trigger the files selection dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddPictures_Click(object sender, RoutedEventArgs e)
        {
            // get the array of file names and insert into the RawFileReader class
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new())
            {
                openFileDialog.Filter = RawFileReader.FilterString;
                openFileDialog.Multiselect = true;
                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
                // add files
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // set input files property in RawFileReader class
                    RawFileReader.AddFiles(openFileDialog.FileNames);
                    RefreshDataGrid();
                }
            }
        }
    }
}
