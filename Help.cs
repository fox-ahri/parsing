using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Template
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            this.webHelp.Url = new Uri(String.Format("file:///{0}/help.html", Directory.GetCurrentDirectory()));
        }
    }
}
