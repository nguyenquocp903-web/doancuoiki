using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnCuoiKi
{
    public partial class MayTinhDungStack : Form
    {
        //=============Tạo giao diện cho máy tính================
        TextBox txtDisplay;
        public MayTinhDungStack()
        {
            InitializeComponent();

            //Tạo TextBox
            txtDisplay = new TextBox();
            txtDisplay.Location = new Point(10, 10);
            txtDisplay.Size = new Size(260, 40);
            txtDisplay.Font = new Font("Segoe UI", 18);
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            this.Controls.Add(txtDisplay);

            //Tạo danh sách nút
            string[] buttons = {
                "(",")","C","<-",
                "1","2","3","/",
                "4","5","6","*",
                "7","8","9","-",
                ".","0","=","+"
            };

            //Chỉnh vị trí cho các nút
            int x = 10, y = 60;
            int w = 60, h = 40;
            int count = 0;

            foreach (string text in buttons)
            {
                Button btn = new Button();
                btn.Text = text;
                btn.Size = new Size(w, h);
                btn.Location = new Point(x, y);

                btn.Click += Button_Click;

                this.Controls.Add(btn);

                x += w + 5;
                count++;

                //cứ 4 nút thì xuống dòng 1 lần
                if (count % 4 == 0)
                {
                    x = 10;
                    y += h + 5;
                }
            }
        }

        //==============Gán chức năng cho nút(in ra text trên button, trừ nút "=")=============
        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Text)
            {
                case "C":
                    txtDisplay.Text = "";
                    break;

                case "<-":
                    if (txtDisplay.Text.Length > 0)
                        txtDisplay.Text = txtDisplay.Text.Remove(txtDisplay.Text.Length - 1);
                    break;

                case "="://Tính toán
                    string a = ChuyenDoi(txtDisplay.Text);
                    if (a == null) txtDisplay.Text = "Lỗi phép tính";

                    else
                    {
                        double kq = DapAn(a);
                        if ( double.IsNaN(kq) ) txtDisplay.Text = "Lỗi chia cho 0";
                        else txtDisplay.Text = kq.ToString("0.##");
                    }
                    break;
                   

                default:
                    txtDisplay.Text += btn.Text;
                    break;
            }
        }

        //======Ứng dụng Stack để tính toán biểu thức toán học đơn giản (chia làm 3 phần)=====

        //1. Hàm ưu tiên (do dấu */ mạnh hơn +-)
        public int UuTien(char c)
        {
            if (c == '+' || c == '-') return 1;
            else if (c == '*' || c == '/') return 2;
            else return 0;
        }

        //2. Hàm chuyển đổi trung tố sang hậu tố
        public string ChuyenDoi(string s)
        {

	s = s.Replace(" ", ""); //xóa khoảng trắng

            char[] a = s.ToCharArray();

            MyStack D = new MyStack();

            string num = "", Out = "";

            for (int i = 0; i < a.Length; i++) //duyệt từng kí tự trong textbox
            {
                char c = a[i];

                //Nếu là số:
                if (char.IsDigit(c) || c == '.')
                {
                    if (c == '.' && num.Contains(".")) return null;
                    num += c;
                }

                //Nếu là dấu:
                else if ("+-*/()".Contains(c))
                {
                    //Các trường hợp có thể xảy ra

                    //a. Biến num khác rỗng(tức là có số)
                    if (num != "")
                    {
                        Out += num + " ";
                        num = "";
                    }

                    //b. Xử lí số âm (ví dụ -3 hoặc (-3))
                    if (c == '-' && (i == 0 || "+-*/(".Contains(a[i - 1])))
                    {
                        num += c;
                        continue;
                    }

                    //c. Phép tính trong ngoặc
                    if (c == '(') D.Push(c);

                    else if (c == ')')
                    {
                        while (!D.IsEmpTy() && (char)D.Peek() != '(')
                        {
                            Out += D.Pop() + " ";
                        }

                        if ( D.IsEmpTy() ) return null; //trường hợp không thấy ngoặc '(' nên nó Pop hết -> lỗi
                        if (!D.IsEmpTy()) D.Pop();
                    }

                    //d. Các phép tính thông thường
                    else
                    {
                        while (!D.IsEmpTy() && UuTien((char)D.Peek()) >= UuTien(c)) //Xét mức độ ưu tiên
                        {
                            Out += D.Pop() + " ";
                        }
                        D.Push(c);
                    }
                }

                //Trường hợp khác ngoài số và dấu toán tử
                else return null;
            }

            //Sàng lọc lại biến num và stack
            if (num != "") Out += num + " ";

            while (!D.IsEmpTy())
            {
                char top = (char)D.Pop();
                if (top == '(') return null; //trường hợp lỗi ngoặc
                Out += top + " ";
            }

            return Out;

        }

        //3. Hàm tính ra đáp án từ chuỗi hậu tố
        public double DapAn(string s)
        {
            char[] a = s.ToCharArray();

            MyStack D = new MyStack();

            string num = "";

            for ( int i = 0; i < a.Length; i++ )
            {
                char c = a[i];

                //khi gặp ' ', tức là đã hết 1 số
                if (c == ' ')
                {
                    if (num == "") continue; //biến num không có gì thì bỏ qua

                    // 1. Nếu chỉ có 1 kí tự và là toán tử 
                    if (num.Length == 1 && "+-*/".Contains(num))
                    {
                        if (D.IsEmpTy()) return double.NaN;
                        double f = (double)D.Pop(); 

                        if (D.IsEmpTy()) return double.NaN;
                        double l = (double)D.Pop();

                        //Lấy số sau thực hiện phép toán với số trước
                        if (num == "+") D.Push(l + f);
                        if (num == "-") D.Push(l - f);
                        if (num == "*") D.Push(l * f);
                        if (num == "/")
                        {
                            if (f  == 0) D.Push(double.NaN); // trường hợp chia 0 thì cho = 0
                            else D.Push(l / f);
                        }
                    }

                    // 2. Nếu là số (ví dụ "5", "1.2" hoặc "-3")
                    else
                    {
                        D.Push(double.Parse(num.ToString()));
                    }

                    num = "";
                }

                //Khi không phải ' ', gom vào biến num
                else num += c;

            }

            if (D.IsEmpTy()) return double.NaN;

            double result = (double)D.Pop();

            // Nếu còn dư dữ liệu -> sai biểu thức
            if (!D.IsEmpTy()) return double.NaN;

            return result;
        }

    }


    //===============Stack================
    public class Node
    {
        public object data;
        public Node next;
    }
    public class MyStack
    {
        private Node top;
        public MyStack()
        {
            top = null;
        }
        public void Push(object d)
        {
            Node n = new Node();
            n.data = d;
            n.next = top;
            top = n;
        }
        public object Pop()
        {
            if (top == null) return null;
            else
            {
                object v = top.data;
                top = top.next;
                return v;
            }
        }
        public bool IsEmpTy()
        {
            return top == null;
        }
        public object Peek()
        {
            if (IsEmpTy()) return null;
            return top.data;
        }

    }
}
