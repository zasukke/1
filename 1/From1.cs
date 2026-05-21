using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Npgsql;

namespace WinFromApp3
{
    public class Form1 : Form
    {
        FlowLayoutPanel flow;
        TextBox txtSearch;
        Button btnAdd, btnDel, btnRefresh;
        string conn = "Host=localhost;Database=WordToys;Username=postgres;Password=";
        string imagePath = @"C:\ProductImages\";

        public Form1()
        {
            Size = new Size(1000, 600);
            Text = "Товары";
            BackColor = Color.White;

            // Кнопки с цветом #DEB887
            txtSearch = new TextBox() { Location = new Point(10, 10), Size = new Size(200, 30) };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            btnAdd = new Button() { Text = "Добавить", Location = new Point(220, 10), Size = new Size(100, 30), BackColor = Color.FromArgb(222, 184, 135) };
            btnAdd.Click += (s, e) => { new AddEditForm(conn).ShowDialog(); LoadProducts(); };

            btnDel = new Button() { Text = "Удалить", Location = new Point(330, 10), Size = new Size(100, 30), BackColor = Color.FromArgb(222, 184, 135) };
            btnDel.Click += BtnDel_Click;

            btnRefresh = new Button() { Text = "Обновить", Location = new Point(440, 10), Size = new Size(100, 30), BackColor = Color.FromArgb(222, 184, 135) };
            btnRefresh.Click += (s, e) => LoadProducts();

            Controls.Add(txtSearch);
            Controls.Add(btnAdd);
            Controls.Add(btnDel);
            Controls.Add(btnRefresh);

            flow = new FlowLayoutPanel() { Dock = DockStyle.Bottom, Height = 520, AutoScroll = true, BackColor = Color.White };
            Controls.Add(flow);

            LoadProducts();
        }

        void LoadProducts()
        {
            flow.Controls.Clear();
            try
            {
                using (NpgsqlConnection c = new NpgsqlConnection(conn))
                {
                    c.Open();
                    string sql = "SELECT artikul, name, categorya, opisanie, proizvoditel, postavshik, prise, unit, kolichestvo, sale, photo FROM Tovar";
                    if (!string.IsNullOrEmpty(txtSearch.Text))
                        sql = "SELECT artikul, name, categorya, opisanie, proizvoditel, postavshik, prise, unit, kolichestvo, sale, photo FROM Tovar WHERE name ILIKE '%" + txtSearch.Text + "%'";

                    NpgsqlDataAdapter a = new NpgsqlDataAdapter(sql, c);
                    DataTable dt = new DataTable();
                    a.Fill(dt);

                    foreach (DataRow r in dt.Rows)
                    {
                        Panel card = new Panel() { Width = 750, Height = 200, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(5) };
                        int stock = Convert.ToInt32(r["kolichestvo"]);
                        int sale = Convert.ToInt32(r["sale"]);

                        // Подсветка строк по ТЗ
                        if (stock == 0) card.BackColor = Color.LightBlue;
                        else if (sale > 17) card.BackColor = Color.FromArgb(255, 222, 173); // #FFDEAD

                        // ===== ФОТО (слева) =====
                        PictureBox pic = new PictureBox() { Size = new Size(150, 150), Location = new Point(10, 20), SizeMode = PictureBoxSizeMode.Zoom };
                        string photoFile = r["photo"]?.ToString();
                        if (!string.IsNullOrEmpty(photoFile) && File.Exists(imagePath + photoFile))
                            pic.Image = Image.FromFile(imagePath + photoFile);
                        else
                            pic.Image = GetPlaceholderImage();
                        card.Controls.Add(pic);

                        // ===== ТЕКСТ ПО ЦЕНТРУ =====
                        int y = 15;
                        int x = 180;

                        // Категория | Наименование
                        Label lblCatName = new Label()
                        {
                            Text = r["categorya"].ToString() + " | " + r["name"].ToString(),
                            Font = new Font("Arial", 11, FontStyle.Bold),
                            Location = new Point(x, y),
                            Size = new Size(500, 25)
                        };
                        card.Controls.Add(lblCatName);
                        y += 28;

                        // Описание
                        Label lblDesc = new Label() { Text = r["opisanie"].ToString(), Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 40) };
                        card.Controls.Add(lblDesc);
                        y += 42;

                        // Производитель
                        Label lblMan = new Label() { Text = "Производитель: " + r["proizvoditel"].ToString(), Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 20) };
                        card.Controls.Add(lblMan);
                        y += 22;

                        // Поставщик
                        Label lblSup = new Label() { Text = "Поставщик: " + r["postavshik"].ToString(), Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 20) };
                        card.Controls.Add(lblSup);
                        y += 22;

                        // Цена
                        decimal price = Convert.ToDecimal(r["prise"]);
                        decimal finalPrice = price * (1 - sale / 100m);
                        string priceText = finalPrice.ToString("F2") + " руб.";
                        if (sale > 0) priceText = price.ToString("F2") + " руб. → " + finalPrice.ToString("F2") + " руб.";
                        Label lblPrice = new Label() { Text = "Цена: " + priceText, Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 20) };
                        card.Controls.Add(lblPrice);
                        y += 22;

                        // Единица измерения
                        Label lblUnit = new Label() { Text = "Единица измерения: " + r["unit"].ToString(), Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 20) };
                        card.Controls.Add(lblUnit);
                        y += 22;

                        // Количество на складе
                        Label lblStock = new Label() { Text = "Количество на складе: " + stock.ToString(), Font = new Font("Arial", 9), Location = new Point(x, y), Size = new Size(500, 20) };
                        if (stock == 0) lblStock.ForeColor = Color.Red;
                        card.Controls.Add(lblStock);

                        // ===== КВАДРАТ СО СКИДКОЙ (справа) =====
                        Panel saleBox = new Panel() { Size = new Size(60, 60), Location = new Point(670, 65), BackColor = sale > 17 ? Color.FromArgb(255, 222, 173) : Color.LightGray, BorderStyle = BorderStyle.FixedSingle };
                        Label lblSaleBox = new Label() { Text = sale + "%", Font = new Font("Arial", 14, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                        saleBox.Controls.Add(lblSaleBox);
                        card.Controls.Add(saleBox);

                        card.Tag = r["artikul"].ToString();
                        card.Click += (s, e) => SelectCard(card);
                        flow.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        Image GetPlaceholderImage()
        {
            Bitmap bmp = new Bitmap(150, 150);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                g.DrawString("Нет фото", new Font("Arial", 10), Brushes.Gray, 40, 65);
            }
            return bmp;
        }

        Panel selected = null;
        void SelectCard(Panel c)
        {
            if (selected != null) selected.BackColor = Color.White;
            selected = c;
            selected.BackColor = Color.LightSteelBlue;
        }

        void BtnDel_Click(object sender, EventArgs e)
        {
            if (selected == null) { MessageBox.Show("Выберите товар"); return; }
            if (MessageBox.Show("Удалить?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (NpgsqlConnection c = new NpgsqlConnection(conn))
                {
                    c.Open();
                    string sql = "DELETE FROM Tovar WHERE artikul = @a";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, c))
                    {
                        cmd.Parameters.AddWithValue("@a", selected.Tag.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadProducts();
            }
        }
    }
}