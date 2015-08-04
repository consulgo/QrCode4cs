using Consulgo.QrCode4cs;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Consulgo.QrCode4Cs.Sample
{
    public class QrViewModel : INotifyPropertyChanged
    {
        private bool[][] _bitmap;
        private string _text;
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand GenerateCommand { get; private set; }

        public bool[][] Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                RaisePropertyChanged("Bitmap");
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        public QrViewModel()
        {
            GenerateCommand = new MyCommand(Generate);
            Text = "Enter your text here";
        }

        private void Generate()
        {
            try
            {
                var qrcode = new QRCode(new Options(Text));
                qrcode.Make();
                var c = qrcode.GetModuleCount();
                var bitmap = new bool[c][];

                for (var row = 0; row < c; row++)
                {
                    bitmap[row] = new bool[c];

                    for (var col = 0; col < c; col++)
                    {
                        var isDark = qrcode.IsDark(row, col);
                        bitmap[row][col] = isDark;
                    }
                }

                Bitmap = bitmap;
            }
            catch (Exception ex)
            {
                Text = "Sorry, an error: " + ex.Message;
                Bitmap = null;
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;

            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
