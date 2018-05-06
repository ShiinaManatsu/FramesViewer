using Digimezzo.WPFControls;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FramesViewer
{
    #region Event Define

    public delegate void IsPlayingEventArgsHandler(object sender, RoutedEventArgs e);
    
    public class RoutedEventArgs:EventArgs
    {

    }

    #endregion

    /// <summary>
    /// Indecate has select folder
    /// </summary>
    public enum PlayerStatus
    {
        NotSetImagePath=0,
        HasSetImagePath = 1,
    }

    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : BorderlessWindows10Window,INotifyPropertyChanged
    {
        #region Dependency Event

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Is playing check box status
        /// </summary>
        private bool? _isPlaying;

        /// <summary>
        /// Instance of bitmap image
        /// </summary>
        private BitmapImage bitmapImage;

        /// <summary>
        /// Instance of seletor
        /// </summary>
        private Seletor seletor;

        /// <summary>
        /// Refresh delay use for Task.Delay() method
        /// </summary>
        private int _refreshTime = 33;

        #endregion

        #region Public Members

        /// <summary>
        /// Fired when is playing status changed
        /// </summary>
        public event IsPlayingEventArgsHandler IsPlayingChanged = (sender, e) => { };

        /// <summary>
        /// Flag for is path selected
        /// </summary>
        public PlayerStatus SetImagePathFlag = PlayerStatus.NotSetImagePath;

        /// <summary>
        /// Fire an IsPlayingChanged event
        /// </summary>
        public void OnIsPlayingChangedChanged()
        {
            IsPlayingChanged?.Invoke(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Current Fps and set the refresh time
        /// </summary>
        public int Fps
        {
            get
            {
                return 1000 / _refreshTime;
            }
            set
            {
                if (value == 0||value>120)
                {
                    return;
                }
                else
                {
                    _refreshTime = 1000 / value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Status of now playing 
        /// </summary>
        public bool? IsPlaying {
            get { return _isPlaying; }
            set { if (_isPlaying != value) { _isPlaying = value;OnPropertyChanged(); } }
        }

        #endregion

        #region Public Function

        /// <summary>
        /// Constructor
        /// </summary>
        public Player()
        {
            InitializeComponent();
            DataContext = this;
            this.Closed += (__, _) => App.Current.Shutdown();
            IsPlaying = IsPlayingToggle.IsChecked;
            IsPlayingToggle.Click += IsPlayingToggle_Click;
            SelectButton.Click += SelectButton_Click;
            seletor = new Seletor(this);
            SetUriTextBlock();           // Set the uri status
            IsPlayingChanged += Play;
            FpsTextBox.PreviewTextInput += IInputx_PreviewTextInput;
            FpsTextBox.TextChanged+= FpsTextBox_PreviewTextInput;
        }

        private void IInputx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var a = (FpsTextBox.Text.Length <= 2) ? true : false;

            if (a)
            {
                e.Handled = IsTextAllowed(e.Text);
            }
            else
            {
                e.Handled = true;
            }
        }
        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");       //regex that matches disallowed text

            return regex.IsMatch(text);
        }

        /// <summary>
        /// Play Frams
        /// </summary>
        public async void PlayAsync()
        {
            string[] files = Directory.GetFiles(seletor.SelectedFolderPath);

            // Loop this
            while (true)
            {
                // Check status
                while (IsPlaying == true && SetImagePathFlag == PlayerStatus.HasSetImagePath)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        // Check and stop when is playing status changed suddenly
                        if (IsPlaying == true)
                        {
                            bitmapImage = null;         // Clear the instance of bitmap to decrease RAM usage
                            bitmapImage = new BitmapImage((new Uri(files[i])));

                            PlayerImageBox.Source = bitmapImage;

                            await Task.Delay(_refreshTime);
                        }
                        else
                        {
                            PlayerImageBox.Source = null;
                            return;
                        }
                    }
                }
                if (IsPlaying == false|| SetImagePathFlag == PlayerStatus.HasSetImagePath)
                {
                    PlayerImageBox.Source = null;
                    return;
                }
            }
        }

        /// <summary>
        /// Set the uri show box text
        /// </summary>
        public void SetUriTextBlock()
        {
            UriShowTextBlock.Text = (seletor.SelectedFolderPath is null) ? "No such folder has selected" : seletor.SelectedFolderPath.ToString();
        }

        #endregion

        #region Private Function

        /// <summary>
        /// FpsTextBox_PreviewTextInput method
        /// Set fps when text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FpsTextBox_PreviewTextInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (FpsTextBox.Text != "")
                {
                    var i = int.Parse(FpsTextBox.Text);
                    if (i <= 120 && i > 0)
                    {
                        Fps = i;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// IsPlayingToggle_Click methos
        /// Change IsPlaying property and fire OnIsPlayingChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsPlayingToggle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsPlaying = IsPlayingToggle.IsChecked;
            OnIsPlayingChangedChanged();
        }

        /// <summary>
        /// Create seletor instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                seletor.Show();
            }
            catch
            {
                seletor = new Seletor(this);
                seletor.Show();
            }
        }

        /// <summary>
        /// Play method
        /// Fires when IsPlaying property is true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play(object sender, RoutedEventArgs e)
        {
            if (IsPlaying == true&&SetImagePathFlag==PlayerStatus.HasSetImagePath)
            {
                PlayAsync();
            }
            else
            {
                return;
            }
        }

        #endregion
    }
}
