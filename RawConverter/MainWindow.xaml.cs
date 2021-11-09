using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        // variables for async events in this class
        private BackgroundWorker backGroundWorkerConvert;
        private static bool buttonIsConvert = true;
        private TimeSpan timeRemaining;
        private bool abortToken = false;

        public MainWindow()
        {
            InitializeComponent();
            ResizeMenuColum(width: defaultMenuWidth);
            DataGridFiles.DataContext = RawFileProcessor.RawFiles;
            SetAppInfo();
            SetCheckboxes();

            // function to set app info
            void SetAppInfo()
            {
                Title = AboutThisApp.name;
                TextBlockInfo.Text = AboutThisApp.name + " V" + AboutThisApp.version + Environment.NewLine + "Bulid date: " + AboutThisApp.buildDate;
            }

            // function to set check boxes
            void SetCheckboxes()
            {
                // input
                switch (RawFileProcessor.InputFileType)
                {
                    case InputFileTypes.orf:
                        CheckBoxORF.IsChecked = true;
                        CheckBoxRAW.IsChecked = false;
                        break;
                    case InputFileTypes.raw:
                        CheckBoxORF.IsChecked = false;
                        CheckBoxRAW.IsChecked = true;
                        break;
                    default:
                        break;
                }

                // ouput
                switch (RawFileProcessor.OutputFileType)
                {
                    case OutputFileTypes.jpg:
                        CheckBoxJPG.IsChecked = true;
                        CheckBoxPNG.IsChecked = false;
                        CheckBoxTIFF.IsChecked = false;
                        break;
                    case OutputFileTypes.png:
                        CheckBoxJPG.IsChecked = false;
                        CheckBoxPNG.IsChecked = true;
                        CheckBoxTIFF.IsChecked = false;
                        break;
                    case OutputFileTypes.tiff:
                        CheckBoxJPG.IsChecked = false;
                        CheckBoxPNG.IsChecked = false;
                        CheckBoxTIFF.IsChecked = true;
                        break;
                    default:
                        break;
                }
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
            /*
            // set source
            DataGridFiles.ItemsSource = RawFileProcessor.dataTableFiles.DefaultView;

            // set column widths
            DataGridLength[] columnWidths = { new DataGridLength(3, DataGridLengthUnitType.Star), new DataGridLength(1, DataGridLengthUnitType.Auto), new DataGridLength(1, DataGridLengthUnitType.Auto) };
            int counter = 0;
            foreach (DataGridColumn column in DataGridFiles.Columns)
            {
                column.Width = columnWidths[counter];
                counter++;
            }       
            */
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
                }
            }
        }

        /// <summary>
        /// Method to change the event for the button convert.
        /// </summary>
        private void ChangeButtonConvert()
        {
            if (buttonIsConvert == true)
            {
                // switch to cancel button
                ImageBrush brush = new()
                {
                    ImageSource = new BitmapFromPath().Load(path: "Resources/Cancel.png")
                };
                ButtonConvert.Background = brush;
                buttonIsConvert = false;
            }
            else
            {
                // switch to convert button
                ImageBrush brush = new()
                {
                    ImageSource = new BitmapFromPath().Load(path: "Resources/convert.png")
                };
                ButtonConvert.Background = brush;
                buttonIsConvert = true;
            }
        }

        /// <summary>
        /// Method to switch the check box states for the output file type.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SwitchCheckBoxState(bool isEnabled)
        {
            _ = CheckBoxJPG.Dispatcher.Invoke(() => CheckBoxJPG.IsEnabled = isEnabled, DispatcherPriority.Background);
            _ = CheckBoxPNG.Dispatcher.Invoke(() => CheckBoxPNG.IsEnabled = isEnabled, DispatcherPriority.Background);
            _ = CheckBoxTIFF.Dispatcher.Invoke(() => CheckBoxTIFF.IsEnabled = isEnabled, DispatcherPriority.Background);
        }


        //////////////////////////////////////////////////////////
        /// Async converting events
        //////////////////////////////////////////////////////////

        /// <summary>
        /// Event for converting the images in the other thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Convert_DoWork(object sender, DoWorkEventArgs e)
        {
            // disable checkboxes
            SwitchCheckBoxState(isEnabled: false);

            // variables for this method
            long timeRemainingMS;
            long totalTimeElapsed = 0;
            long totalTime;

            // convert each file in the raw file list
            int counter = 1;
            foreach (RawFileProcessor.RawFile rawFile in RawFileProcessor.RawFiles)
            {
                // check if process was aborted by user
                if (backGroundWorkerConvert.CancellationPending == true)
                {
                    // abort requested
                    e.Cancel = true;
                    SwitchCheckBoxState(isEnabled: true);
                    break;
                }
                else
                {
                    // new object of type stopwatch
                    Stopwatch watch = new();
                    // take the time for converting
                    watch.Start();

                    // convert
                    rawFile.Convert();

                    // rename file
                    string oldName = RawFileProcessor.OutputFolder + "\\" + rawFile.Name + rawFile.Extension;
                    string newName = RawFileProcessor.OutputFolder + "\\" + rawFile.Name + "." + RawFileProcessor.OutputFileType.ToString();
                    File.Move(sourceFileName: oldName, destFileName: newName, overwrite: true);

                    // current job percentage
                    // this must be a double value in order to prevent the percentage being 0 in case file count is >100
                    double percentProgress = (double)counter / RawFileProcessor.RawFiles.Count * 100;

                    // stop time for stopwatch
                    watch.Stop();

                    // total time elapsed
                    totalTimeElapsed += watch.ElapsedMilliseconds;

                    // calculate remaining time in every loop to get best value possible
                    totalTime = (long)(Convert.ToDouble(100) / percentProgress * totalTimeElapsed);
                    timeRemainingMS = totalTime - totalTimeElapsed;
                    timeRemaining = TimeSpan.FromMilliseconds(timeRemainingMS);


                    // set counter
                    counter++;

                    // report progress
                    backGroundWorkerConvert.ReportProgress((int)percentProgress, timeRemaining);
                }
            }
        }

        /// <summary>
        /// Event raised when progress in async method changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Convert_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string timeString = string.Format("{0:#00}:{1:#00}:{2:#00}", timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
            LabelConvertProgress.Content = $"Progress {e.ProgressPercentage}% with {timeString} remaining.";
            ProgressBarConvert.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Event raised when process is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Export_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // set flag for button is convert
            //buttonIsConvert = true;
            ChangeButtonConvert();

            // reset is convert button flag and check boxes
            SwitchCheckBoxState(isEnabled: true);

            // check if process was aborted
            if (abortToken == true)
            {
                // process was aborted by user
                LabelConvertProgress.Content = "Aborted";
                ProgressBarConvert.Value = 100;

                // reset token
                abortToken = false;
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
                // folder for output is selected
                if (RawFileProcessor.RawFiles.Count == 0)
                {
                    // data table is empty
                    // no raw files staged for converting
                    string caption = AboutThisApp.name;
                    string messageBoxText = "No images loaded.";
                    _ = MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // activate background process or cancel it
                    if (buttonIsConvert == true)
                    {
                        // change button appearance
                        ChangeButtonConvert();

                        // NEW ASYNC
                        // set flag for running process
                        RawFileProcessor.ProcessIsRunning = true;
                        // new backgroundworker
                        backGroundWorkerConvert = new()
                        {
                            WorkerReportsProgress = true,
                            WorkerSupportsCancellation = true
                        };
                        backGroundWorkerConvert.DoWork += Convert_DoWork;
                        backGroundWorkerConvert.RunWorkerCompleted += Export_RunWorkerCompleted;
                        backGroundWorkerConvert.ProgressChanged += Convert_ProgressChanged;

                        // run the background worker
                        LabelConvertProgress.Content = "Initializing...";
                        backGroundWorkerConvert.RunWorkerAsync();
                    }
                    else
                    {
                        // CANCEL ASYNC
                        backGroundWorkerConvert.CancelAsync();
                        abortToken = true;
                        RawFileProcessor.ProcessIsRunning = false;
                    }
                }
            }
            else
            {
                // error message for missing destination folder
                string caption = AboutThisApp.name;
                string messageBoxText = "Please select a destination folder.";
                _ = MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
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

        /// <summary>
        /// Event to trigger methods to remove the selected item(s) from the datatable and datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemRemovedSelected_Click(object sender, RoutedEventArgs e)
        {  
            // get selected items
            IList selectedItems = DataGridFiles.SelectedItems;
      
            // extract the indices
            List<int> selectedIndices = new();
            foreach (RawFileProcessor.RawFile rawFile in selectedItems)
            {
                // add the index to the list
                int index = RawFileProcessor.RawFiles.IndexOf(rawFile);
                selectedIndices.Add(index);
            }
            
            // remove the files and update the gui
            RawFileProcessor.RemoveFiles(selectedIndices);        
        }

        /// <summary>
        /// Event to clear the files and refresh the data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemClearAll_Click(object sender, RoutedEventArgs e)
        {
            RawFileProcessor.RemoveAllFiles();
        }

        /// <summary>
        /// Event to open the AboutWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonMoreInfo_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        /// <summary>
        /// Event to trigger setting change when checkbox raw is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxRAW_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.InputFileType = "raw";
            UserSettings.Default.Save();
            CheckBoxORF.IsChecked = false;
        }

        /// <summary>
        /// Event to trigger setting change when checkbox orf is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxORF_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.InputFileType = "orf";
            UserSettings.Default.Save();
            CheckBoxRAW.IsChecked = false;
        }

        /// <summary>
        /// Event to trigger setting change when checkbox jpg is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxJPG_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.OutputFileType = "jpg";
            UserSettings.Default.Save();
            CheckBoxPNG.IsChecked = false;
            CheckBoxTIFF.IsChecked = false;
        }

        /// <summary>
        /// Event to trigger setting change when checkbox png is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxPNG_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.OutputFileType = "png";
            UserSettings.Default.Save();
            CheckBoxJPG.IsChecked = false;
            CheckBoxTIFF.IsChecked = false;
        }

        /// <summary>
        /// Event to trigger setting change when checkbox tiff is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxTIFF_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.OutputFileType = "tiff";
            UserSettings.Default.Save();
            CheckBoxPNG.IsChecked = false;
            CheckBoxJPG.IsChecked = false;
        }
    }
}
