// 
// Summary:
//
// Playing with roguelike programming concepts using a Panel on a Windows Form
// and DrawString to draw the map from a multidimensional array.
//
// Description:
//
// In an effort to learn the programming techniques used in Roguelike games
// I present the following code. As with anything this is a work in progess
// and things are always in a state of flux.
//
// So far this code draws a very simple room based dungeon, places some $
// at random locations and the player character @ can move around, grab the $
// and get points added to his score.
//
// Frank Hale <frankhale@gmail.com>
// 16 December 2012
//
// GNU GPLv3 quick guide: http://www.gnu.org/licenses/quick-guide-gplv3.html
// GNU GPLv3 license <http://www.gnu.org/licenses/gpl-3.0.html>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SillyGame
{
	public class BufferedPanel : Panel
	{
		public BufferedPanel()
		{
			DoubleBuffered = true;
		}
	}

	public class BSPTreeNode<T>
	{
		public T Data { get; set; }

		public BSPTreeNode<T> Parent { get; set; }
		public BSPTreeNode<T> Left { get; set; }
		public BSPTreeNode<T> Right { get; set; }
	}

	public class BSPTree<T>
	{
		private BSPTreeNode<T> rootLeaf;

		public BSPTree()
		{
			rootLeaf = new BSPTreeNode<T>();
		}

		public void AddLeaf(T data)
		{
			bool done = false;
			BSPTreeNode<T> currLeaf = rootLeaf;

			while (!done)
			{
				if (currLeaf.Left != null)
					currLeaf = currLeaf.Left;
				else if (currLeaf.Left != null)
					currLeaf = currLeaf.Right;

				BSPTreeNode<T> newLeaf = new BSPTreeNode<T>();
				newLeaf.Parent = currLeaf;

				if (currLeaf.Left == null)
					currLeaf.Left = newLeaf;
				else if (currLeaf.Right == null)
					currLeaf.Right = newLeaf;

				currLeaf.Data = data;
			}
		}

	}

	public enum RewardType
	{
		Normal,
		Special
	}

	public interface IScoreable
	{
		int Score { get; }
	}

	// Eventually all objects on the map will be of type Cell
	public abstract class Cell
	{
		public string Character { get; protected set; }
		public Point Location { get; set; }
		public Color Color { get; protected set; }
	}

	public class Reward : Cell, IScoreable
	{
		public int Score { get; private set; }
		public RewardType Type { get; private set; }

		public Reward(int score, RewardType type, Point location)
		{
			Score = score;
			Type = type;
			Character = "$";
			Location = location;

			if (Type == RewardType.Normal)
				Color = Color.Red;
			else if (Type == RewardType.Special)
				Color = Color.Yellow;
		}
	}

	public class RogueLike
	{
		private Font gameFont = new Font("Consolas", 18);
		private Font infoFont = new Font("Consolas", 8);
		private int gameFontHeight;
		private int gameFontWidth;
		private string[,] map = new string[30, 20];
		private List<Reward> rewards = new List<Reward>();
		private string player = "@";
		private string wall = "#";
		private string floor = ".";
		private int score = 0;
		private Point playerLocation = new Point(3, 3);
		private BufferedPanel mapPanel;
		private Form mainForm;

		public RogueLike(Form form, BufferedPanel panel)
		{
			mainForm = form;
			mainForm.Text = "Not Quite A Roguelike (Yet)";
			mapPanel = panel;
			mainForm.KeyDown += mainForm_KeyDown;
			mapPanel.Paint += mapPanel_Paint;
			gameFontHeight = gameFont.Height - 8;
			gameFontWidth = Convert.ToInt32(panel.CreateGraphics().MeasureString("#", gameFont).Width) - 8;
			fillRewards();
			fillMap();
		}

		protected void mapPanel_Paint(object sender, PaintEventArgs e)
		{
			Color color = Color.White;

			for (int mx = 0; mx < map.GetLength(0); mx++)
			{
				for (int my = 0; my < map.GetLength(1); my++)
				{
					var r = rewards.FirstOrDefault(x => x.Location == new Point(mx, my));

					if (r != null)
					{
						color = r.Color;
					}
					else if (map[mx, my] == wall)
						color = Color.BurlyWood;
					else
						color = Color.White;

					e.Graphics.DrawString(map[mx, my], gameFont, new SolidBrush(color), new Point(mx * gameFontWidth, my * gameFontHeight));
				}
			}

			e.Graphics.DrawString(string.Format("Score: {0}", score), gameFont, new SolidBrush(Color.White), new Point(430, 10));
			drawInfo(e.Graphics);
		}

		protected void mainForm_KeyDown(object sender, KeyEventArgs e)
		{
			int pointX = playerLocation.X;
			int pointY = playerLocation.Y;

			if (e.KeyCode == Keys.Up)
			{
				if (!(playerLocation.Y - 1 < 0))
					pointY = playerLocation.Y - 1;
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (playerLocation.Y + 1 != map.GetLength(1))
					pointY = playerLocation.Y + 1;
			}
			else if (e.KeyCode == Keys.Left)
			{
				if (!(playerLocation.X - 1 < 0))
					pointX = playerLocation.X - 1;
			}
			else if (e.KeyCode == Keys.Right)
			{
				if (playerLocation.X + 1 != map.GetLength(0))
					pointX = playerLocation.X += 1;
			}

			if (map[pointX, pointY] != wall)
			{
				playerLocation.X = pointX;
				playerLocation.Y = pointY;

				fillMap();
				mapPanel.Invalidate();
			}
		}

		private void fillRewards()
		{
			Random numReward = new Random();
			Random rewardPlacement = new Random();

			for (int i = 0; i < numReward.Next(10, 20); i++)
			{
				rewards.Add(new Reward(10, RewardType.Normal, new Point(rewardPlacement.Next(2, map.GetLength(0) - 1), rewardPlacement.Next(2, map.GetLength(1) - 1))));
			}

			rewards.Add(new Reward(25, RewardType.Special, new Point(rewardPlacement.Next(2, map.GetLength(0) - 1), rewardPlacement.Next(2, map.GetLength(1) - 1))));
		}

		private void fillMap()
		{
			for (int x = 0; x < map.GetLength(0); x++)
			{
				for (int y = 0; y < map.GetLength(1); y++)
				{
					if ((x == 0) || (y == 0))
					{
						map[x, y] = wall;
					}
					else if ((x == map.GetLength(0) - 1) || (y == map.GetLength(1) - 1))
					{
						map[x, y] = wall;
					}
					else
					{
						foreach (var r in rewards)
						{
							map[r.Location.X, r.Location.Y] = r.Character;
						}

						map[x, y] = floor;
					}

					var reward = rewards.FirstOrDefault(r => r.Location.X == playerLocation.X && r.Location.Y == playerLocation.Y);

					if (reward != null)
					{
						score += reward.Score;
						map[playerLocation.X, playerLocation.Y] = floor;

						rewards.Remove(reward);

						if (rewards.Count() == 0)
						{
							fillRewards();
							fillMap();
							mapPanel.Invalidate();
						}
					}

					map[playerLocation.X, playerLocation.Y] = player;
				}
			}
		}

		private void drawInfo(Graphics g)
		{
			g.DrawLine(new Pen(Brushes.White, 4), new Point(430, 50), new Point(625, 50));
			g.DrawString("red $ are worth 10 points", infoFont, new SolidBrush(Color.White), new Point(430, 70));
			g.DrawString("yellow $ are worth 25 points", infoFont, new SolidBrush(Color.White), new Point(430, 80));
		}
	}
}
