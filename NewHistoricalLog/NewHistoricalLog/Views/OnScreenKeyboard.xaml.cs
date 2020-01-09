using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;


namespace NewHistoricalLog.Views
{
    /// <summary>
    /// Interaction logic for OnScreenKeyboard.xaml
    /// </summary>
    public partial class OnScreenKeyboard : ThemedWindow
    {
        public string Value { get; set; } = "";

        public OnScreenKeyboard()
        {
            InitializeComponent();
            Loaded += OnScreenKeyboard_Loaded;
            keyboard.WrittingFinished += Keyboard_WrittingFinished;
            Deactivated += OnScreenKeyboard_Deactivated;
            Closing += OnScreenKeyboard_Closing;
            KeyDown += OnScreenKeyboard_KeyDown;
        }

        private void OnScreenKeyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnScreenKeyboard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Deactivated -= OnScreenKeyboard_Deactivated;
        }

        private void OnScreenKeyboard_Deactivated(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch
            {
                NLog.LogManager.GetCurrentClassLogger().Error("Ошибка закрытия окна клавиатуры!");
            }
        }

        private void Keyboard_WrittingFinished(object sender, string writtingResult)
        {
            Value = keyboard.Result;
            Close();
        }

        private void OnScreenKeyboard_Loaded(object sender, RoutedEventArgs e)
        {
            keyboard.CurrentText = Value;
        }
    }
}
