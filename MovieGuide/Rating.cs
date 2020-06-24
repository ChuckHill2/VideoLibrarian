using System.Windows.Forms;

namespace MovieGuide
{
    public partial class Rating : UserControl
    {
        public Rating()
        {
            InitializeComponent();
        }

        public float Value
        {
            get
            {
                return float.Parse(m_lblRating.Text);
            }
            set
            {
                m_lblRating.Text = value.ToString();
            }
        }
    }
}
