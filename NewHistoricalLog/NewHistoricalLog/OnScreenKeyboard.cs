using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace NewHistoricalLog
{
    public partial class OnScreenKeyboard : DevExpress.XtraEditors.XtraForm
    {
        Boolean capslock;
        Boolean rus;
        int maximumchars = 80;

        public OnScreenKeyboard()
        {
            InitializeComponent();
            this.TopLevel = true;
        }

        public string GetText()
        {
            return textBox1.Text;
        }

        private void button50_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < maximumchars)
            {
                textBox1.Text = textBox1.Text + (sender as Button).Text;
            }

        }

        private void Onscreenkeyboard_Deactivate(object sender, EventArgs e)
        {
            //Close();           
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (!capslock)
            {
                capslock = true;
                button16.Text = button16.Text.ToUpper();
                button17.Text = button17.Text.ToUpper();
                button18.Text = button18.Text.ToUpper();
                button19.Text = button19.Text.ToUpper();
                button20.Text = button20.Text.ToUpper();
                button21.Text = button21.Text.ToUpper();
                button22.Text = button22.Text.ToUpper();
                button23.Text = button23.Text.ToUpper();
                button24.Text = button24.Text.ToUpper();
                button25.Text = button25.Text.ToUpper();
                if (rus)
                {
                    button26.Text = button26.Text.ToUpper();
                    button27.Text = button27.Text.ToUpper();
                }

                button30.Text = button30.Text.ToUpper();
                button31.Text = button31.Text.ToUpper();
                button32.Text = button32.Text.ToUpper();
                button33.Text = button33.Text.ToUpper();
                button34.Text = button34.Text.ToUpper();
                button35.Text = button35.Text.ToUpper();
                button36.Text = button36.Text.ToUpper();
                button37.Text = button37.Text.ToUpper();
                button38.Text = button38.Text.ToUpper();
                if (rus)
                {
                    button39.Text = button39.Text.ToUpper();
                    button40.Text = button40.Text.ToUpper();
                }

                button44.Text = button44.Text.ToUpper();
                button45.Text = button45.Text.ToUpper();
                button46.Text = button46.Text.ToUpper();
                button47.Text = button47.Text.ToUpper();
                button48.Text = button48.Text.ToUpper();
                button49.Text = button49.Text.ToUpper();
                button50.Text = button50.Text.ToUpper();
                if (rus)
                {
                    button51.Text = button51.Text.ToUpper();
                    button52.Text = button52.Text.ToUpper();
                }
            }
            else
            {
                capslock = false;
                button16.Text = button16.Text.ToLower();
                button17.Text = button17.Text.ToLower();
                button18.Text = button18.Text.ToLower();
                button19.Text = button19.Text.ToLower();
                button20.Text = button20.Text.ToLower();
                button21.Text = button21.Text.ToLower();
                button22.Text = button22.Text.ToLower();
                button23.Text = button23.Text.ToLower();
                button24.Text = button24.Text.ToLower();
                button25.Text = button25.Text.ToLower();
                if (rus)
                {
                    button26.Text = button26.Text.ToLower();
                    button27.Text = button27.Text.ToLower();
                }

                button30.Text = button30.Text.ToLower();
                button31.Text = button31.Text.ToLower();
                button32.Text = button32.Text.ToLower();
                button33.Text = button33.Text.ToLower();
                button34.Text = button34.Text.ToLower();
                button35.Text = button35.Text.ToLower();
                button36.Text = button36.Text.ToLower();
                button37.Text = button37.Text.ToLower();
                button38.Text = button38.Text.ToLower();
                if (rus)
                {
                    button39.Text = button39.Text.ToLower();
                    button40.Text = button40.Text.ToLower();
                }

                button44.Text = button44.Text.ToLower();
                button45.Text = button45.Text.ToLower();
                button46.Text = button46.Text.ToLower();
                button47.Text = button47.Text.ToLower();
                button48.Text = button48.Text.ToLower();
                button49.Text = button49.Text.ToLower();
                button50.Text = button50.Text.ToLower();
                if (rus)
                {
                    button51.Text = button51.Text.ToLower();
                    button52.Text = button52.Text.ToLower();
                }
            }

        }

        private void button69_Click(object sender, EventArgs e)
        {
            rus = true;
            capslock = false;
            button16.Text = "й";
            button17.Text = "ц";
            button18.Text = "у";
            button19.Text = "к";
            button20.Text = "е";
            button21.Text = "н";
            button22.Text = "г";
            button23.Text = "ш";
            button24.Text = "щ";
            button25.Text = "з";
            button26.Text = "х";
            button27.Text = "ъ";

            button30.Text = "ф";
            button31.Text = "ы";
            button32.Text = "в";
            button33.Text = "а";
            button34.Text = "п";
            button35.Text = "р";
            button36.Text = "о";
            button37.Text = "л";
            button38.Text = "д";
            button39.Text = "ж";
            button40.Text = "э";

            button44.Text = "я";
            button45.Text = "ч";
            button46.Text = "с";
            button47.Text = "м";
            button48.Text = "и";
            button49.Text = "т";
            button50.Text = "ь";
            button51.Text = "б";
            button52.Text = "ю";
        }

        private void button70_Click(object sender, EventArgs e)
        {
            capslock = false;
            rus = false;
            button16.Text = "q";
            button17.Text = "w";
            button18.Text = "e";
            button19.Text = "r";
            button20.Text = "t";
            button21.Text = "y";
            button22.Text = "u";
            button23.Text = "i";
            button24.Text = "o";
            button25.Text = "p";
            button26.Text = "[";
            button27.Text = "]";

            button30.Text = "a";
            button31.Text = "s";
            button32.Text = "d";
            button33.Text = "f";
            button34.Text = "g";
            button35.Text = "h";
            button36.Text = "j";
            button37.Text = "k";
            button38.Text = "l";
            button39.Text = ";";
            button40.Text = "'";

            button44.Text = "z";
            button45.Text = "x";
            button46.Text = "c";
            button47.Text = "v";
            button48.Text = "b";
            button49.Text = "n";
            button50.Text = "m";
            button51.Text = ",";
            button52.Text = ".";
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0) { textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 1, 1); }
        }

        private void button64_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < maximumchars)
            {
                if (textBox1.Text.Length > 0)
                { textBox1.Text = textBox1.Text + " "; }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}