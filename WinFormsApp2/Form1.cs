using System.Text;
using WinFormsApp2.functions;
namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string FileName = textBox1.Text;
            string? Filecontent=textBox2.Text;
            byte[] Contentbuf = Encoding.Default.GetBytes(Filecontent??"Default content");            
            byte[] Copybuf;            
            Functions.FileCreate(FileName);
            Functions.writeToDisk(Contentbuf,Encoding.Default);
            byte[] buf = Functions.ReadDrive(512);
            Functions.CopyMemory(buf,out  Copybuf);
            string text = Encoding.Default.GetString(Copybuf);            
            text=text.ToLower();
            char[] charArray = text.ToCharArray();
            // Сортируем массив символов в обратном алфавитном порядке
            Array.Sort(charArray);
            Array.Reverse(charArray);
            string reversedString = new string(charArray);            
                 
            byte[] resultbuf = Encoding.Default.GetBytes(reversedString);
            Functions.writeToDisk(resultbuf,Encoding.Default);
            string MapViewResults = Functions.MapView();
            //вывод на консоль
            label2.Text= text;
            label3.Text = reversedString;
            label4.Text = MapViewResults;
        }

    }
}
