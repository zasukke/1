using System;
using System.Windows.Forms;
using Npgsql;

namespace WinFromApp3
{
    public class EditForm : Form
    {
        TextBox txtName, txtPrice, txtStock, txtSale;
        Label lblArt;
        Button btnSave;
        string connStr;
        string oldArt;

        public EditForm(string conn, string artikel)
        {
            connStr = conn;
            oldArt = artikel;
            Size = new Size(350, 350);
            Text = "Изменить товар";

            int y = 10;

            // Артикул (только для чтения)
            Controls.Add(new Label() { Text = "Артикул:", Location = new Point(10, y), Size = new Size(100, 25) });
            lblArt = new Label() { Text = artikel, Location = new Point(120, y), Size = new Size(150, 25), BackColor = System.Drawing.Color.LightGray };
            Controls.Add(lblArt);
            y += 35;

            txtName = new TextBox() { Location = new Point(120, y), Size = new Size(150, 25) };
            Controls.Add(new Label() { Text = "Название:", Location = new Point(10, y), Size = new Size(100, 25) });
            Controls.Add(txtName);
            y += 35;

            txtPrice = new TextBox() { Location = new Point(120, y), Size = new Size(150, 25) };
            Controls.Add(new Label() { Text = "Цена:", Location = new Point(10, y), Size = new Size(100, 25) });
            Controls.Add(txtPrice);
            y += 35;

            txtStock = new TextBox() { Location = new Point(120, y), Size = new Size(150, 25) };
            Controls.Add(new Label() { Text = "Кол-во:", Location = new Point(10, y), Size = new Size(100, 25) });
            Controls.Add(txtStock);
            y += 35;

            txtSale = new TextBox() { Location = new Point(120, y), Size = new Size(150, 25) };
            Controls.Add(new Label() { Text = "Скидка %:", Location = new Point(10, y), Size = new Size(100, 25) });
            Controls.Add(txtSale);
            y += 40;

            btnSave = new Button() { Text = "Сохранить", Location = new Point(100, y), Size = new Size(100, 35) };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            LoadData();
        }

        void LoadData()
        {
            using (NpgsqlConnection c = new NpgsqlConnection(connStr))
            {
                c.Open();
                string sql = "SELECT * FROM Tovar WHERE artikul = @a";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@a", oldArt);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtName.Text = r["name"].ToString();
                            txtPrice.Text = r["prise"].ToString();
                            txtStock.Text = r["kolichestvo"].ToString();
                            txtSale.Text = r["sale"].ToString();
                        }
                    }
                }
            }
        }

        void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Заполните название");
                return;
            }

            try
            {
                using (NpgsqlConnection c = new NpgsqlConnection(connStr))
                {
                    c.Open();
                    string sql = "UPDATE Tovar SET name=@n, prise=@p, kolichestvo=@s, sale=@d WHERE artikul=@a";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, c))
                    {
                        cmd.Parameters.AddWithValue("@a", oldArt);
                        cmd.Parameters.AddWithValue("@n", txtName.Text);
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@s", int.Parse(txtStock.Text));
                        cmd.Parameters.AddWithValue("@d", int.Parse(txtSale.Text));
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Товар изменён!");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}