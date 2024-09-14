namespace ChessBoardExample
{
    public partial class ChessBoardExample : Form
    {
        const int gridSize = 32; // 每个小方块大小为40像素
        const int step = 3; // 递进

        Dictionary<string, Panel> chessboardDict = new Dictionary<string, Panel>();
        HashSet<Panel> redPanelsSet = new HashSet<Panel>();

        public ChessBoardExample()
        {
            InitializeComponent();
            InitializeChessboard();

            //foreach (var panel in chessboardDict.Values)
            //{
            //    panel.MouseEnter += OnCellMouseEnter;
            //    panel.MouseLeave += OnCellMouseLeave;
            //}

            string key = "5_9";
            Button b = new Button();
            b.Size = new Size(gridSize, gridSize);
            b.Text = "";
            chessboardDict[key].Controls.Add(b);
            b.Click += new EventHandler((sender, args) =>
            {
                if (redPanelsSet.Count > 0) SetOriginalCells();
                else CheckRedCells(5, 9);
            });

            Size = new Size(562, 562);
        }

        private void InitializeChessboard()
        {
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    AddNewGrid(i, j);
        }

        private void AddNewGrid(int x, int y)
        {
            string key = x.ToString() + "_" + y.ToString();
            var p = new Point(x * gridSize, y * gridSize);

            Panel cell = new Panel()
            {
                Name = key,
                Size = new Size(gridSize, gridSize),
                Location = p,
                BorderStyle = BorderStyle.Fixed3D,
            };
            if ((x + y) % 2 == 0)
                cell.BackColor = Color.White;
            else
                cell.BackColor = Color.LightGray;

            Controls.Add(cell);

            chessboardDict[key] = cell;
        }

        private void CheckRedCells(int x, int y)
        {
            try
            {
                SetOriginalCells();
                HashSet<string> keysToSet = new HashSet<string>();

                for (int dx = -step; dx <= step; ++dx)
                {
                    for (int dy = -step; dy <= step; ++dy)
                    {
                        if (Math.Abs(dx) + Math.Abs(dy) <= step)
                        {//限制在中心点周围范围内
                            if (x + dx >= -15 && x + dx < 16 && y + dy >= -15 && y + dy < 16)
                            {//检查是否在棋盘范围内
                                string key = (x + dx) + "_" + (y + dy);
                                if (chessboardDict.ContainsKey(key))
                                    keysToSet.Add(key);//将符合条件的坐标添加到集合中
                            }
                        }
                    }
                }
                this.SetRedCells(keysToSet); // 调用设置红色背景方法，传递新计算出来的键集合
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetRedCells(HashSet<string> keysToSet)
        {
            foreach (var k in keysToSet)
            {
                var coords = k.Split('_');
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                var npn = chessboardDict[x + "_" + y];
                npn.BackColor = Color.FromArgb(128, Color.Red);//设置红色面板

                redPanelsSet.Add(npn);
            }
        }

        private void SetOriginalCells()
        {
            try
            {
                foreach (var panel in redPanelsSet)
                {
                    string[] coords = panel.Name.Split('_');
                    int x = int.Parse(coords[0]);
                    int y = int.Parse(coords[1]);

                    if ((x + y) % 2 == 0)
                        panel.BackColor = Color.White;
                    else
                        panel.BackColor = Color.LightGray; // 恢复为原来的背景颜色
                }

                redPanelsSet.Clear(); // 清空红色面板集合
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnCellMouseEnter(object? sender, System.EventArgs e)
        {
            try
            {
                if (sender is null) return;
                // 获取当前鼠标进入区域对应格子位置；
                string[] s = ((Panel)sender).Name.Split('_');
                int x = int.Parse(s[0]);
                int y = int.Parse(s[1]);

                CheckRedCells(x, y);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnCellMouseLeave(object? sender, EventArgs e)
        {
            SetOriginalCells();
        }
    }
}
