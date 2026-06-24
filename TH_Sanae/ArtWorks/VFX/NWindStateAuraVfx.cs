using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.ArtWorks.VFX
{
	public sealed partial class NWindStateAuraVfx : Node2D
	{
		private readonly struct CloudItem
		{
			public readonly Sprite2D Sprite;
			public readonly float Phase;
			public readonly float Scale;
			public readonly float RadiusScaleX;
			public readonly float RadiusScaleY;
			public readonly float Drift;

			public CloudItem(Sprite2D sprite, float phase, float scale, float radiusScaleX, float radiusScaleY, float drift)
			{
				Sprite = sprite;
				Phase = phase;
				Scale = scale;
				RadiusScaleX = radiusScaleX;
				RadiusScaleY = radiusScaleY;
				Drift = drift;
			}
		}

		private readonly List<CloudItem> _clouds = new List<CloudItem>();
		private Vector2 _baseCenter;
		private double _elapsed;
		private Texture2D? _cloudTex;
		private Creature? _owner;

		public int RootZIndex { get; set; } = 0;

		public int CloudCount { get; set; } = 9;

		public float RadiusX { get; set; } = 150f;

		public float RadiusY { get; set; } = 48f;

		public float AngularSpeed { get; set; } = 1.2f;

		public float BaseAlpha { get; set; } = 0.55f;

		public float HeightOffsetPixels { get; set; } = 135f;

		public Color CloudColor { get; set; } = new Color(0.88f, 0.92f, 0.98f, 1f);

		public static NWindStateAuraVfx Create(Creature owner)
		{
			var vfx = new NWindStateAuraVfx();
			vfx._owner = owner;
			return vfx;
		}

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			ZAsRelative = false;
			ZIndex = RootZIndex;

			_cloudTex = CreateCloudTexture(128);
			_baseCenter = new Vector2(0f, -HeightOffsetPixels);

			int n = Mathf.Clamp(CloudCount, 3, 24);
			for (int i = 0; i < n; i++)
			{
				var s = new Sprite2D();
				s.Texture = _cloudTex;
				s.Centered = true;
				s.ZAsRelative = false;
				s.ZIndex = 0;
				AddChild(s);

				float phase = (Mathf.Tau * i / n) + (GD.Randf() * 0.35f);
				float scale = Mathf.Lerp(0.70f, 1.35f, GD.Randf());
				float rsx = Mathf.Lerp(0.85f, 1.25f, GD.Randf());
				float rsy = Mathf.Lerp(0.75f, 1.20f, GD.Randf());
				float drift = Mathf.Lerp(4f, 14f, GD.Randf());

				s.Scale = new Vector2(scale * 1.15f, scale * 0.85f);
				s.Modulate = CloudColor;

				_clouds.Add(new CloudItem(s, phase, scale, rsx, rsy, drift));
			}
		}

		public override void _Process(double delta)
		{
			_elapsed += delta;

			if (_owner == null || _owner.IsDead || !_owner.HasPower<WindStatePower>())
			{
				Visible = false;
				QueueFree();
				return;
			}

			if (NCombatRoom.Instance == null)
			{
				Visible = false;
				return;
			}

			var creatureNode = NCombatRoom.Instance.GetCreatureNode(_owner);
			if (creatureNode == null)
			{
				Visible = false;
				return;
			}

			Visible = true;
			GlobalPosition = creatureNode.Hitbox.GlobalPosition + new Vector2(creatureNode.Hitbox.Size.X / 2f, creatureNode.Hitbox.Size.Y);
			_baseCenter = new Vector2(0f, -HeightOffsetPixels);

			float rx = Math.Max(10f, RadiusX);
			float ry = Math.Max(6f, RadiusY);

			float a0 = (float)_elapsed * AngularSpeed;
			for (int i = 0; i < _clouds.Count; i++)
			{
				CloudItem c = _clouds[i];
				float ang = a0 + c.Phase;

				float x = Mathf.Cos(ang) * rx * c.RadiusScaleX;
				float y = Mathf.Sin(ang) * ry * c.RadiusScaleY;

				float drift = Mathf.Sin(((float)_elapsed * 0.9f) + c.Phase) * c.Drift;
				c.Sprite.Position = _baseCenter + new Vector2(x, y + drift);

				float depth = Mathf.Sin(ang);
				float alpha = Mathf.Clamp(BaseAlpha * (0.45f + 0.55f * depth), 0.12f, 0.85f);

				Color m = c.Sprite.Modulate;
				m.A = alpha;
				c.Sprite.Modulate = m;
				c.Sprite.ZIndex = -2 + (int)Mathf.Round(depth * 4f);
			}
		}

		private static Texture2D CreateCloudTexture(int size)
		{
			int s = Math.Max(32, size);
			var img = Image.CreateEmpty(s, s, false, Image.Format.Rgba8);

			float cx = (s - 1) * 0.5f;
			float cy = (s - 1) * 0.5f;
			float r = s * 0.5f;

			for (int y = 0; y < s; y++)
			{
				for (int x = 0; x < s; x++)
				{
					float dx = (x - cx) / r;
					float dy = (y - cy) / r;
					float d = Mathf.Sqrt(dx * dx + dy * dy);

					float a = Mathf.Clamp(1f - d, 0f, 1f);
					a = a * a * (3f - 2f * a);
					a *= 0.95f;

					img.SetPixel(x, y, new Color(1f, 1f, 1f, a));
				}
			}

			return ImageTexture.CreateFromImage(img);
		}
	}
}
