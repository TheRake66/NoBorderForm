using NoBorderForm.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoBorderForm
{
    static class Librairies
    {
        // ====================================================================
        public enum formCustomButton : int
        {
            MinimizeCroix = 0,
            MaximizeCroix = 1,
            OnlyCroix = 2,
            Alls = 3
        }
        public static IntPtr formCustomInit(
            Form uneForm, 
            Color backColor,
            Color couleurActive, 
            Color couleurInactive, 
            Color couleurButtons,
            bool paintBar,
            Control forButton,
            formCustomButton desButtons
            )
        {
            // -------------------------------------
            int invisible = 10;
            int bord = 1;


            // Modifi la form
            uneForm.FormBorderStyle = FormBorderStyle.None;
            uneForm.TransparencyKey = Color.FromArgb(1, 0, 0);
            uneForm.BackColor = uneForm.TransparencyKey;
            uneForm.Padding = new Padding(invisible);

            // Bordures
            Panel border = new Panel();
            border.Dock = DockStyle.Fill;
            border.BackColor = couleurActive;
            border.Padding = new Padding(bord);
            uneForm.Controls.Add(border);
            uneForm.Activated += new EventHandler((e, s) => 
            { 
                border.BackColor = couleurActive;
                if (paintBar) forButton.BackColor = couleurActive; 
            });
            uneForm.Deactivate += new EventHandler((e, s) => 
            { 
                border.BackColor = couleurInactive;
                if (paintBar) forButton.BackColor = couleurInactive; 
            });

            // Container
            Panel cont = new Panel();
            cont.Dock = DockStyle.Fill;
            cont.BackColor = backColor;
            border.Controls.Add(cont);

            // Button croix
            Button cross = new Button();
            cross.Font = new Font("Marlett", 12F);
            cross.Size = new Size(50, 30);
            cross.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cross.Text = "r";
            cross.ForeColor = couleurButtons;
            cross.FlatStyle = FlatStyle.Flat;
            cross.FlatAppearance.BorderSize = 0;
            cross.Location = new Point(forButton.Right - invisible - 50, 0);
            cross.Click += new EventHandler((e, s) => { uneForm.Close(); });
            cross.MouseEnter += new EventHandler((e, s) => { cross.BackColor = Color.FromArgb(232, 17, 35); });
            cross.MouseLeave += new EventHandler((e, s) => { cross.BackColor = Color.Transparent; });
            forButton.Controls.Add(cross);

            // Button maximizer
            if (desButtons == formCustomButton.Alls || desButtons == formCustomButton.MaximizeCroix)
            {
                Button maximize = new Button();
                maximize.Font = new Font("Marlett", 12F);
                maximize.Size = new Size(50, 30);
                maximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                maximize.Text = "1";
                maximize.ForeColor = couleurButtons;
                maximize.FlatStyle = FlatStyle.Flat;
                maximize.FlatAppearance.BorderSize = 0;
                maximize.Location = new Point(forButton.Right - invisible - 100, 0);
                maximize.Click += new EventHandler((e, s) =>
                {
                    if (maximize.FindForm().WindowState == FormWindowState.Maximized)
                    {
                        uneForm.Padding = new Padding(invisible);
                        border.Padding = new Padding(bord);
                        maximize.Text = "1";
                        maximize.FindForm().WindowState = FormWindowState.Normal;
                    }
                    else
                    {
                        uneForm.Padding = new Padding(0);
                        border.Padding = new Padding(0);
                        maximize.Text = "2";
                        maximize.FindForm().WindowState = FormWindowState.Maximized;
                    }
                });

                uneForm.SizeChanged += new EventHandler((e, s) =>
                {
                    if (maximize.FindForm().WindowState == FormWindowState.Maximized)
                    {
                        uneForm.Padding = new Padding(0);
                        border.Padding = new Padding(0);
                        maximize.Text = "2";
                    }
                    else
                    {
                        uneForm.Padding = new Padding(invisible);
                        border.Padding = new Padding(bord);
                        maximize.Text = "1";
                    }
                });

                // Double click sur la barre
                forButton.DoubleClick += new EventHandler((e, s) =>
                { 
                    if (uneForm.WindowState == FormWindowState.Maximized) uneForm.WindowState = FormWindowState.Normal;
                    else uneForm.WindowState = FormWindowState.Maximized;
                });
                forButton.Controls.Add(maximize);
            }

            // Button minimizer
            if (desButtons == formCustomButton.Alls || desButtons == formCustomButton.MinimizeCroix)
            {
                Button minimize = new Button();
                minimize.Font = new Font("Marlett", 12F);
                minimize.Size = new Size(50, 30);
                minimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                minimize.Text = "0";
                minimize.ForeColor = couleurButtons;
                minimize.FlatStyle = FlatStyle.Flat;
                minimize.FlatAppearance.BorderSize = 0;
                minimize.Location = new Point(forButton.Right - invisible - (desButtons == formCustomButton.Alls ? 150:100), 0);
                minimize.Click += new EventHandler((e, s) =>
                {
                    uneForm.WindowState = FormWindowState.Minimized;
                });
                forButton.Controls.Add(minimize);
            }

            // Ajout des controls à la sous form
            for (int i = 0; i < uneForm.Controls.Count; i++)
            {
                if (uneForm.Controls[i] != cont && uneForm.Controls[i] != border)
                {
                    uneForm.Controls[i].Parent = cont;
                    i--; // PAS DE FOREACH --> a chaque setparent le controls.count diminu
                }
            }


            // Gère le déplacement de la fenetre
            bool IfIsMoveForm = false;
            Point MoveFormStart = new Point();
            forButton.MouseDown += new MouseEventHandler((s, e) =>
            {
                IfIsMoveForm = true;
                MoveFormStart = new Point(e.X, e.Y);
            });
            forButton.MouseUp += new MouseEventHandler((s, e) =>
            {
                IfIsMoveForm = false;

                // Gère la fenetre dans les coins
                int minx = Screen.FromPoint(Cursor.Position).Bounds.X;
                int miny = Screen.FromPoint(Cursor.Position).Bounds.Y;
                int maxx = minx + Screen.FromPoint(Cursor.Position).Bounds.Width - 1;
                int maxy = miny + Screen.FromPoint(Cursor.Position).Bounds.Height - 1;
                int maxw = Screen.FromPoint(Cursor.Position).WorkingArea.Width;
                int maxh = Screen.FromPoint(Cursor.Position).WorkingArea.Height;

                // Haut gauche
                bool addborder = false;
                if (Cursor.Position.X == minx &&
                    Cursor.Position.Y == miny)
                {
                    uneForm.Location = new Point(minx, miny);
                    uneForm.Size = new Size(maxw / 2, maxh / 2);
                    addborder = true;
                }
                // Haut droit
                else if (Cursor.Position.X == maxx &&
                    Cursor.Position.Y == miny)
                {
                    uneForm.Location = new Point(maxx - (maxw / 2), miny);
                    uneForm.Size = new Size(maxw / 2, maxh / 2);
                    addborder = true;
                }
                // Bas droit
                else if (Cursor.Position.X == maxx &&
                    Cursor.Position.Y == maxy)
                {
                    uneForm.Location = new Point(maxx - (maxw / 2), maxy - (maxh / 2));
                    uneForm.Size = new Size(maxw / 2, maxh / 2);
                    addborder = true;
                }
                // Bas gauche
                else if (Cursor.Position.X == minx &&
                    Cursor.Position.Y == maxy)
                {
                    uneForm.Location = new Point(minx, maxy - (maxh / 2));
                    uneForm.Size = new Size(maxw / 2, maxh / 2);
                    addborder = true;
                }
                // Gauche
                else if (Cursor.Position.X == minx)
                {
                    uneForm.Location = new Point(minx, miny);
                    uneForm.Size = new Size(maxw / 2, maxh);
                    addborder = true;
                }
                // Droite
                else if (Cursor.Position.X == maxx)
                {
                    uneForm.Location = new Point(maxx - (maxw / 2), miny);
                    uneForm.Size = new Size(maxw / 2, maxh);
                    addborder = true;
                }
                // Haut
                else if (Cursor.Position.Y == miny && (desButtons == formCustomButton.Alls || desButtons == formCustomButton.MaximizeCroix))
                {
                    uneForm.WindowState = FormWindowState.Maximized;
                }
                // Bas
                else if (Cursor.Position.Y == maxy && (desButtons == formCustomButton.Alls || desButtons == formCustomButton.MinimizeCroix))
                {
                    uneForm.Location = new Point(maxw / 2 - uneForm.Width / 2, maxh / 2 - uneForm.Height / 2);
                }

                if (addborder)
                {
                    uneForm.Location = new Point(uneForm.Location.X - invisible, uneForm.Location.Y - invisible);
                    uneForm.Size = new Size(uneForm.Width + invisible * 2, uneForm.Height + invisible * 2);
                }
            });
            forButton.MouseMove += new MouseEventHandler((s, e) =>
            {
                if (IfIsMoveForm)
                {
                    Point p = uneForm.PointToScreen(e.Location);
                    uneForm.Location = new Point(p.X - MoveFormStart.X, p.Y - MoveFormStart.Y);
                }
            });

            // Gère le redimenssionnement
            bool inresize = false;
            int resizemode = 0;
            uneForm.MouseDown += new MouseEventHandler((a, b) => { inresize = true; });
            uneForm.MouseUp += new MouseEventHandler((a, b) => { inresize = false; });
            uneForm.MouseLeave += new EventHandler((a, b) => { uneForm.Cursor = Cursors.Default; });
            uneForm.MouseMove += new MouseEventHandler((a, b) =>
            {
                if (inresize)
                {
                    int newx = uneForm.Location.X;
                    int newy = uneForm.Location.Y;
                    int neww = uneForm.Width;
                    int newh = uneForm.Height;

                    if (resizemode == 0)
                    {
                        // Haut gauche
                        newx = uneForm.Location.X + b.X;
                        newy = uneForm.Location.Y + b.Y;
                        neww = uneForm.Size.Width + -(b.X);
                        newh = uneForm.Size.Height + -(b.Y);
                    }
                    else if (resizemode == 1)
                    {
                        // Bas gauche
                        newx = uneForm.Location.X + b.X;
                        newy = uneForm.Location.Y;
                        neww = uneForm.Size.Width + -(b.X);
                        newh = b.Y;
                    }
                    else if (resizemode == 2)
                    {
                        // Haut droit
                        newx = uneForm.Location.X;
                        newy = uneForm.Location.Y + b.Y;
                        neww = b.X;
                        newh = uneForm.Size.Height + -(b.Y);
                    }
                    else if (resizemode == 3)
                    {
                        // Bas droit
                        neww = b.X;
                        newh = b.Y;
                    }
                    else if (resizemode == 4)
                    {
                        // Gauche
                        newx = uneForm.Location.X + b.X;
                        newy = uneForm.Location.Y;
                        neww = uneForm.Size.Width + -(b.X);
                        newh = uneForm.Size.Height;
                    }
                    else if (resizemode == 5)
                    {
                        // Droite
                        neww = b.X;
                        newh = uneForm.Size.Height;
                    }
                    else if (resizemode == 6)
                    {
                        // Haut
                        newx = uneForm.Location.X;
                        newy = uneForm.Location.Y + b.Y;
                        neww = uneForm.Size.Width;
                        newh = uneForm.Size.Height + -(b.Y);
                    }
                    else if (resizemode == 7)
                    {
                        // Bas
                        neww = uneForm.Size.Width;
                        newh = b.Y;
                    }


                    if ((uneForm.Height == 2 ||
                        uneForm.Height == uneForm.MinimumSize.Height ||
                        uneForm.Height == uneForm.MaximumSize.Height) && newh < uneForm.Height)
                    {
                        newh = uneForm.Height;
                        newy = uneForm.Location.Y;
                    }
                    
                    if ((uneForm.Width == 2 ||
                        uneForm.Width == uneForm.MinimumSize.Width ||
                        uneForm.Width == uneForm.MaximumSize.Width) && neww < uneForm.Width)
                    {
                        neww = uneForm.Width;
                        newx = uneForm.Location.X;
                    }

                    uneForm.Location = new Point(newx, newy);
                    uneForm.Size = new Size(neww, newh);
                    uneForm.Refresh();
                }
                else
                {
                    if (b.X < 20 && b.Y < 20)
                    {
                        // Haut gauche
                        uneForm.Cursor = Cursors.SizeNWSE;
                        resizemode = 0;
                    }
                    else if (b.X < 20 && b.Y > uneForm.Height - 20)
                    {
                        // Bas gauche
                        uneForm.Cursor = Cursors.SizeNESW;
                        resizemode = 1;
                    }
                    else if (b.X > uneForm.Width - 20 && b.Y < 20)
                    {
                        // Haut droit
                        uneForm.Cursor = Cursors.SizeNESW;
                        resizemode = 2;
                    }
                    else if (b.X > uneForm.Width - 20 && b.Y > uneForm.Height - 20)
                    {
                        // Bas droit
                        uneForm.Cursor = Cursors.SizeNWSE;
                        resizemode = 3;
                    }
                    else if (b.X < 20)
                    {
                        // Gauche
                        uneForm.Cursor = Cursors.SizeWE;
                        resizemode = 4;
                    }
                    else if (b.X > uneForm.Width - 20)
                    {
                        // Droite
                        uneForm.Cursor = Cursors.SizeWE;
                        resizemode = 5;
                    }
                    else if (b.Y < 20)
                    {
                        // Haut
                        uneForm.Cursor = Cursors.SizeNS;
                        resizemode = 6;
                    }
                    else if (b.Y > uneForm.Height - 20)
                    {
                        // Bas
                        uneForm.Cursor = Cursors.SizeNS;
                        resizemode = 7;
                    }
                }
            });

            return cont.Handle;
            // -------------------------------------
        }
        private static void formCustomResizeStop()
        {
            // -------------------------------------
            //this.Cursor = Cursors.Default;
            // -------------------------------------
        }
        // ====================================================================
    }
}