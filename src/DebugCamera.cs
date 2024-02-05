using System;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000485 RID: 1157
	public class DebugCamera : BasePerspectiveCamera
	{
		// Token: 0x06001AFF RID: 6911 RVA: 0x0001168D File Offset: 0x0000F88D
		public DebugCamera(GameWidget view) : base(view)
		{
            m_primitivesRenderer2d = new PrimitivesRenderer2D();
		}

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x06001B00 RID: 6912 RVA: 0x00004B27 File Offset: 0x00002D27
		public override bool UsesMovementControls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06001B01 RID: 6913 RVA: 0x000031CC File Offset: 0x000013CC
		public override bool IsEntityControlEnabled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001B02 RID: 6914 RVA: 0x000116B8 File Offset: 0x0000F8B8
		public override void Activate(Camera previousCamera)
		{
			this.m_position = previousCamera.ViewPosition;
			this.m_direction = previousCamera.ViewDirection;
			base.SetupPerspectiveCamera(this.m_position, this.m_direction, Vector3.UnitY);
		}

		// Token: 0x06001B03 RID: 6915 RVA: 0x000BA3B0 File Offset: 0x000B85B0
		public override void Update(float dt)
		{
			Vector3 zero = Vector3.Zero;
			if (Keyboard.IsKeyDown(Key.A))
			{
				zero.X = -1f;
			}
			if (Keyboard.IsKeyDown(Key.D))
			{
				zero.X = 1f;
			}
			if (Keyboard.IsKeyDown(Key.W))
			{
				zero.Z = 1f;
			}
			if (Keyboard.IsKeyDown(Key.S))
			{
				zero.Z = -1f;
			}
			Vector2 vector = 0.03f * new Vector2((float)Mouse.MouseMovement.X, (float)(-(float)Mouse.MouseMovement.Y));
			bool flag = Keyboard.IsKeyDown(Key.Shift);
			bool flag2 = Keyboard.IsKeyDown(Key.Control);
			Vector3 direction = this.m_direction;
			Vector3 unitY = Vector3.UnitY;
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(direction, unitY));
			float num = 8f;
			if (flag)
			{
				num *= 10f;
			}
			if (flag2)
			{
				num /= 10f;
			}
			Vector3 vector3 = Vector3.Zero;
			vector3 += num * zero.X * vector2;
			vector3 += num * zero.Y * unitY;
			vector3 += num * zero.Z * direction;
			this.m_position += vector3 * dt;
			this.m_direction = Vector3.Transform(this.m_direction, Matrix.CreateFromAxisAngle(unitY, -4f * vector.X * dt));
			this.m_direction = Vector3.Transform(this.m_direction, Matrix.CreateFromAxisAngle(vector2, 4f * vector.Y * dt));
			base.SetupPerspectiveCamera(this.m_position, this.m_direction, Vector3.UnitY);
			Vector2 v = base.GameWidget.ActualSize / 2f;
			FlatBatch2D flatBatch2D = m_primitivesRenderer2d.FlatBatch(0, DepthStencilState.None, null, null);
			int count = flatBatch2D.LineVertices.Count;
			flatBatch2D.QueueLine(v - new Vector2(5f, 0f), v + new Vector2(5f, 0f), 0f, Color.White);
			flatBatch2D.QueueLine(v - new Vector2(0f, 5f), v + new Vector2(0f, 5f), 0f, Color.White);
			flatBatch2D.TransformLines(Matrix.Identity, count, -1);
		}

		// Token: 0x04001466 RID: 5222
		public static string AmbientParameters = string.Empty;

		// Token: 0x04001467 RID: 5223
		private Vector3 m_position;

        public PrimitivesRenderer2D m_primitivesRenderer2d;
		// Token: 0x04001468 RID: 5224
		private Vector3 m_direction;
	}
}
