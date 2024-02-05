using System;
using System.Collections.Generic;
using Game;

namespace API_WE_Mod
{
	public class Maze
	{
		public Maze(int width, int height)
		{
			this.random = new Game.Random();
			this.InstRooms(width, height);
			this.OrganizeRooms();
			this.SetFixedDoor();
			this.Interlink();
		}

		private void InstRooms(int width, int height)
		{
			this.roomMatrix = new Maze.Room[width, height];
			this.roads = new List<List<Maze.Room>>();
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					List<Maze.Room> list = new List<Maze.Room>();
					this.roomMatrix[i, j] = new Maze.Room();
					list.Add(this.roomMatrix[i, j]);
					this.roads.Add(list);
				}
			}
		}

		private void OrganizeRooms()
		{
			for (int i = 0; i < this.roomMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < this.roomMatrix.GetLength(1) - 1; j++)
				{
					this.roomMatrix[i, j].BottonDoor = this.roomMatrix[i, j + 1].TopDoor;
				}
			}
			for (int k = 0; k < this.roomMatrix.GetLength(1); k++)
			{
				for (int l = 0; l < this.roomMatrix.GetLength(0) - 1; l++)
				{
					this.roomMatrix[l, k].RightDoor = this.roomMatrix[l + 1, k].LeftDoor;
				}
			}
		}

		private void SetFixedDoor()
		{
			for (int i = 0; i < this.roomMatrix.GetLength(0); i++)
			{
				this.roomMatrix[i, 0].TopDoor.IsFixed = true;
			}
			for (int j = 0; j < this.roomMatrix.GetLength(0); j++)
			{
				this.roomMatrix[j, this.roomMatrix.GetLength(1) - 1].BottonDoor.IsFixed = true;
			}
			for (int k = 0; k < this.roomMatrix.GetLength(1); k++)
			{
				this.roomMatrix[0, k].LeftDoor.IsFixed = true;
			}
			for (int l = 0; l < this.roomMatrix.GetLength(1); l++)
			{
				this.roomMatrix[this.roomMatrix.GetLength(0) - 1, l].RightDoor.IsFixed = true;
			}
		}

		private void Interlink()
		{
			while (!this.AllRoomLinked())
			{
				List<Maze.Door> outlineDoors = this.GetOutlineDoors(this.roads[this.random.Int(0, 1048575) % this.roads.Count]);
				Maze.Door door = outlineDoors[this.random.Int(0, 1048575) % outlineDoors.Count];
				List<List<Maze.Room>> oldRoads = this.GetOldRoads(door);
				List<Maze.Room> newRoad = this.GetNewRoad(oldRoads);
				this.RemoveOldRoads(oldRoads);
				this.roads.Add(newRoad);
				door.OpenTheDoor();
			}
		}

		private void RemoveOldRoads(List<List<Maze.Room>> oldRoads)
		{
			foreach (List<Maze.Room> item in oldRoads)
			{
				this.roads.Remove(item);
			}
		}

		private List<Maze.Room> GetNewRoad(List<List<Maze.Room>> oldRoad)
		{
			List<Maze.Room> list = new List<Maze.Room>();
			foreach (List<Maze.Room> list2 in oldRoad)
			{
				foreach (Maze.Room item in list2)
				{
					list.Add(item);
				}
			}
			return list;
		}

		private List<List<Maze.Room>> GetOldRoads(Maze.Door door)
		{
			List<List<Maze.Room>> list = new List<List<Maze.Room>>();
			foreach (List<Maze.Room> list2 in this.roads)
			{
				foreach (Maze.Room room in list2)
				{
					if (this.TwoDoorAreEqual(room.TopDoor, door))
					{
						list.Add(list2);
						break;
					}
					if (this.TwoDoorAreEqual(room.BottonDoor, door))
					{
						list.Add(list2);
						break;
					}
					if (this.TwoDoorAreEqual(room.LeftDoor, door))
					{
						list.Add(list2);
						break;
					}
					if (this.TwoDoorAreEqual(room.RightDoor, door))
					{
						list.Add(list2);
						break;
					}
				}
			}
			return list;
		}

		private bool TwoDoorAreEqual(Maze.Door doorSource, Maze.Door doorTarget)
		{
			return doorSource.GetLockState() && !doorSource.IsFixed && object.Equals(doorSource, doorTarget);
		}

		private bool AllRoomLinked()
		{
			return this.roads.Count == 1;
		}

		private List<Maze.Door> GetOutlineDoors(List<Maze.Room> road)
		{
			List<Maze.Door> list = new List<Maze.Door>();
			foreach (Maze.Room room in road)
			{
				this.AddOutlineDoor(room.TopDoor, list);
				this.AddOutlineDoor(room.BottonDoor, list);
				this.AddOutlineDoor(room.LeftDoor, list);
				this.AddOutlineDoor(room.RightDoor, list);
			}
			return list;
		}

		private void AddOutlineDoor(Maze.Door door, List<Maze.Door> outlineDoors)
		{
			if (!door.GetLockState() || door.IsFixed)
			{
				return;
			}
			if (!outlineDoors.Contains(door))
			{
				outlineDoors.Add(door);
				return;
			}
			outlineDoors.Remove(door);
		}

		private bool[,] RoomToData()
		{
			bool[,] array = new bool[this.roomMatrix.GetLength(0) * 2 + 1, this.roomMatrix.GetLength(1) * 2 + 1];
			this.PreFill(array);
			for (int i = 0; i < this.roomMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < this.roomMatrix.GetLength(1); j++)
				{
					this.SetData(array, i, j, -1, 0, this.roomMatrix[i, j].LeftDoor.GetLockState());
					this.SetData(array, i, j, 1, 0, this.roomMatrix[i, j].RightDoor.GetLockState());
					this.SetData(array, i, j, 0, -1, this.roomMatrix[i, j].TopDoor.GetLockState());
					this.SetData(array, i, j, 0, 1, this.roomMatrix[i, j].BottonDoor.GetLockState());
				}
			}
			return array;
		}

		private void SetData(bool[,] dataMatrix, int xPos, int yPos, int xOffset, int yOffset, bool isClose)
		{
			dataMatrix[xPos * 2 + 1 + xOffset, yPos * 2 + 1 + yOffset] = isClose;
		}

		private void PreFill(bool[,] dataMatrix)
		{
			for (int i = 0; i < dataMatrix.GetLength(0); i += 2)
			{
				for (int j = 0; j < dataMatrix.GetLength(1); j += 2)
				{
					dataMatrix[i, j] = true;
				}
			}
		}

		public bool[,] GetBoolArray()
		{
			return this.RoomToData();
		}

		private Maze.Room[,] roomMatrix;

		private List<List<Maze.Room>> roads;

		private Game.Random random;

		private class Room
		{
			public Maze.Door TopDoor
			{
				get
				{
					return this.topDoor;
				}
				set
				{
					this.topDoor = value;
				}
			}

			public Maze.Door BottonDoor
			{
				get
				{
					return this.bottonDoor;
				}
				set
				{
					this.bottonDoor = value;
				}
			}

			public Maze.Door LeftDoor
			{
				get
				{
					return this.leftDoor;
				}
				set
				{
					this.leftDoor = value;
				}
			}

			public Maze.Door RightDoor
			{
				get
				{
					return this.rightDoor;
				}
				set
				{
					this.rightDoor = value;
				}
			}

			public Maze.Door TopLeftDoor
			{
				get
				{
					return this.topLeftDoor;
				}
				set
				{
					this.topLeftDoor = value;
				}
			}

			public Maze.Door TopRightDoor
			{
				get
				{
					return this.topRightDoor;
				}
				set
				{
					this.topRightDoor = value;
				}
			}

			public Maze.Door BottonLeftDoor
			{
				get
				{
					return this.bottonLeftDoor;
				}
				set
				{
					this.bottonLeftDoor = value;
				}
			}

			public Maze.Door BottonRightDoor
			{
				get
				{
					return this.bottonRightDoor;
				}
				set
				{
					this.bottonRightDoor = value;
				}
			}

			public Room()
			{
				this.topDoor = new Maze.Door();
				this.bottonDoor = new Maze.Door();
				this.leftDoor = new Maze.Door();
				this.rightDoor = new Maze.Door();
				this.topLeftDoor = new Maze.Door();
				this.topRightDoor = new Maze.Door();
				this.bottonLeftDoor = new Maze.Door();
				this.bottonRightDoor = new Maze.Door();
			}

			private Maze.Door topDoor;

			private Maze.Door bottonDoor;

			private Maze.Door leftDoor;

			private Maze.Door rightDoor;

			private Maze.Door topLeftDoor;

			private Maze.Door topRightDoor;

			private Maze.Door bottonLeftDoor;

			private Maze.Door bottonRightDoor;
		}

		private class Door
		{
			public bool IsFixed
			{
				get
				{
					return this.isFixed;
				}
				set
				{
					this.isFixed = value;
				}
			}

			public Door()
			{
				this.isLocked = true;
			}

			public void OpenTheDoor()
			{
				this.isLocked = false;
			}

			public void CloseTheDoor()
			{
				this.isLocked = true;
			}

			public bool GetLockState()
			{
				return this.isLocked;
			}

			private bool isLocked;

			private bool isFixed;
		}
	}
}
