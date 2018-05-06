using Digimezzo.WPFControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Seletor.xaml
    /// </summary>
    public partial class Seletor : BorderlessWindows10Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Instance of main window
        /// </summary>
        private Player mPlayer;

        /// <summary>
        /// Selected Folder Path
        /// </summary>
        public string SelectedFolderPath { get; set; }

        /// <summary>
        /// INotifyPropertyChanged Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// A flaf that indicate has it finished select
        /// </summary>
        public bool IsFinished { get; set; } = false;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="player"></param>
        public Seletor(Player player)
        {
            InitializeComponent();
            DataContext = this;
            mPlayer = player;
            this.SelectButton.Click += SelectButton_Clcke;
            this.FinishButton.Click += FinishButton_Clicke;
        }

        /// <summary>
        /// OnPropertyChanged Mothod
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// SelectButton_Clcke method
        /// To show the file dialog and set the SelectedFolderPath
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectButton_Clcke(object sender, System.Windows.RoutedEventArgs e)
        {
            new Thread(() =>
              {
                  using (var f = new OpenFileDialog() { CheckFileExists = false, CheckPathExists = false, ValidateNames = false, FileName = "Select Folder" })
                  {
                      f.ShowDialog();
                      SelectedFolderPath = f.FileName.Remove(f.FileName.Length - 13);
                  }
              })
            { ApartmentState = ApartmentState.STA }.Start();
        }

        /// <summary>
        /// FinishButton_Clicke method
        /// When finished select, this will set the status of player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishButton_Clicke(object sender, System.Windows.RoutedEventArgs e)
        {
            if(SelectedFolderPath != null)
            {
                SetStatus();
                this.Hide();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// SetStatus method
        /// </summary>
        private void SetStatus()
        {
            mPlayer.SetImagePathFlag = PlayerStatus.HasSetImagePath;
            mPlayer.UriShowTextBlock.Text = (SelectedFolderPath is null) ? "No such folder has selected" : SelectedFolderPath.ToString();
        }
    }
}
