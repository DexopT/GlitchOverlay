using System.Windows;

namespace GlitchOverlay
{
    /// <summary>
    /// Dialog for entering a new preset name
    /// </summary>
    public partial class PresetNameDialog : Window
    {
        public string PresetName { get; private set; }

        public PresetNameDialog()
        {
            InitializeComponent();
            PresetNameTextBox.Focus();
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            PresetName = PresetNameTextBox.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(PresetName))
            {
                MessageBox.Show("Please enter a preset name.", "Invalid Name", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PresetNameTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Set focus to text box after window is fully loaded
            PresetNameTextBox.Focus();
            PresetNameTextBox.SelectAll();
        }
    }
}
