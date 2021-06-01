using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        private static bool menuVisible = false;

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

        //////////////////////////////////////////////////////////
        /// Private methods
        //////////////////////////////////////////////////////////

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
            DataGridFiles.ItemsSource = RawFileProcessor.dataTableFiles.DefaultView;

            // set column widths
            DataGridLength[] columnWidths = { new DataGridLength(3, DataGridLengthUnitType.Star), new DataGridLength(1, DataGridLengthUnitType.Auto), new DataGridLength(1, DataGridLengthUnitType.Auto) };
            int counter = 0;
            foreach (DataGridColumn column in DataGridFiles.Columns)
            {
                column.Width = columnWidths[counter];
            }       
        }

        /// <summary>
        /// Method to call the open file dialog to select images.
        /// </summary>
        private void AddFiles()
        {
            // get the array of file names and insert into the RawFileReader class
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new())
            {
                openFileDialog.Filter = RawFileProcessor.FilterString;
                openFileDialog.Multiselect = true;
                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
                // add files
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // set input files property in RawFileReader class
                    RawFileProcessor.AddFiles(openFileDialog.FileNames);
                    RefreshDataGrid();
                }
            }
        }

        //////////////////////////////////////////////////////////
        /// Events
        //////////////////////////////////////////////////////////

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
            AddFiles();
        }

        /// <summary>
        /// Event to trigger converting selected images.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            if (RawFileProcessor.OutputFolder != null & RawFileProcessor.OutputFolder != "")
            {
                if (RawFileProcessor.dataTableFiles.Rows.Count == 0)
                {
                    // data table is empty
                    string caption = AboutThisApp.name;
                    string messageBoxText = "No images loaded.";
                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // convert all files

                    // set the maximum for the progress bar
                    ProgressBarConvert.Maximum = RawFileProcessor.listRawFiles.Count;

                    // convert each file in the raw file list
                    int counter = 1;
                    foreach (RawFileProcessor.RawFile rawFile in RawFileProcessor.listRawFiles)
                    {
                        // convert
                        rawFile.Convert();

                        // rename file
                        string oldName = RawFileProcessor.OutputFolder + "\\" + rawFile.Name + rawFile.Extension;
                        string newName = RawFileProcessor.OutputFolder + "\\" + rawFile.Name + "." + RawFileProcessor.OutputFileType.ToString();
                        File.Move(sourceFileName: oldName, destFileName: newName, overwrite: true);

                        // set the progress bar and label by using the dispatcher property
                        // change this into a backgourdworker process. "asynchronos coding" in new thread to avoid freezing.
                        float progressFraction = (float)counter / RawFileProcessor.listRawFiles.Count;
                        double percentageRounded = Math.Round(progressFraction * 100, 2, MidpointRounding.AwayFromZero);
                        _ = LabelConvertProgress.Dispatcher.Invoke(() => LabelConvertProgress.Content = $"Progress {percentageRounded}% {rawFile.Name}{rawFile.Extension}", DispatcherPriority.Background);
                        _ = ProgressBarConvert.Dispatcher.Invoke(() => ProgressBarConvert.Value = counter, DispatcherPriority.Background);

                        // set counter
                        counter++;
                    }
                }
            }
            else
            {
                string caption = AboutThisApp.name;
                string messageBoxText = "Please select a destination folder.";
                MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Event to trigger the folder browser dialog for the output path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new();

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // selection was made
                RawFileProcessor.OutputFolder = folderBrowserDialog.SelectedPath;

                // set label and tooltip
                LabelOutputFolder.Content = "Current: " + RawFileProcessor.OutputFolder;
                LabelOutputFolder.ToolTip = RawFileProcessor.OutputFolder;
            }
        }

        /// <summary>
        /// Event to trigger the add files methods.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAddItems_Click(object sender, RoutedEventArgs e)
        {
            AddFiles();
        }

        private void MenuItemRemovedSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Event to clear the files and refresh the data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemClearAll_Click(object sender, RoutedEventArgs e)
        {
            RawFileProcessor.RemoveAllFiles();
            RefreshDataGrid();
        }
    }
}
