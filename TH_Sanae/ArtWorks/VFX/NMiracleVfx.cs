using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using System;
using System.Reflection;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

public partial class NMiracleVfx : Node2D
{
	private const string AdditiveMatPath = "res://TH_Sanae/ArtWorks/VFX/canvas_item_material_additive_shared.tres";
	private const float StartingDuration = 0.7f;
	private const int TextureSize = 192;

	private static Material? _additiveMat;
	private static Texture2D? _miracleTexture;

	private Sprite2D? _outerSprite;
	private Sprite2D? _innerSprite;
	private Creature? _target;
	private float _duration = StartingDuration;
	private bool _playSfx = true;

	public Color PrimaryColor { get; set; } = StsColors.gold;
	public Color SecondaryColor { get; set; } = new Color(1f, 0.6f, 0.2f, 0f);
	public Vector2 OffsetPixels { get; set; } = new Vector2(0f, -160f);

	public static NMiracleVfx? Create(Creature? target, Vector2? offsetPixels = null, Color? primaryColor = null, Color? secondaryColor = null, bool playSfx = true)
	{
		if (TestMode.IsOn)
		{
			return null;
		}

		var vfx = new NMiracleVfx();
		vfx._target = target;
		vfx._playSfx = playSfx;
		if (offsetPixels.HasValue)
		{
			vfx.OffsetPixels = offsetPixels.Value;
		}
		if (primaryColor.HasValue)
		{
			vfx.PrimaryColor = primaryColor.Value;
		}
		if (secondaryColor.HasValue)
		{
			vfx.SecondaryColor = secondaryColor.Value;
		}

		return vfx;
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		TopLevel = true;
		ZAsRelative = false;
		ZIndex = 40;

		_additiveMat ??= ResourceLoader.Load<Material>(AdditiveMatPath);
		_miracleTexture ??= CreateMiracleTexture(TextureSize);

		_outerSprite = new Sprite2D
		{
			Name = "OuterSprite",
			Centered = true,
			Texture = _miracleTexture,
			Material = _additiveMat
		};
		AddChild(_outerSprite);

		_innerSprite = new Sprite2D
		{
			Name = "InnerSprite",
			Centered = true,
			Texture = _miracleTexture,
			Material = _additiveMat
		};
		AddChild(_innerSprite);

		RefreshPosition();
		ApplyVisuals();

		if (_playSfx)
		{
			SfxCmd.Play(FmodSfx.heal);
		}
	}

	public override void _Process(double delta)
	{
		RefreshPosition();

		_duration -= (float)delta;
		if (_duration <= 0f)
		{
			QueueFree();
			return;
		}

		ApplyVisuals();
	}

	private void RefreshPosition()
	{
		Vector2? center = ResolveCreatureCenter(_target);
		if (center.HasValue)
		{
			GlobalPosition = center.Value + OffsetPixels;
		}
	}

	private void ApplyVisuals()
	{
		if (_outerSprite == null || _innerSprite == null)
		{
			return;
		}

		float alpha = ResolveAlpha();
		float durationRatio = Mathf.Clamp(_duration / StartingDuration, 0f, 1f);
		float scale = 2.4f + (0.3f - 2.4f) * Mathf.Pow(durationRatio, 5f);

		Color outerColor = SecondaryColor;
		outerColor.A = alpha;
		_outerSprite.Modulate = outerColor;
		_outerSprite.Scale = Vector2.One * (scale * 1.1f);

		Color innerColor = PrimaryColor;
		innerColor.A = alpha;
		_innerSprite.Modulate = innerColor;
		_innerSprite.Scale = Vector2.One * (scale * 0.9f);
	}

	private float ResolveAlpha()
	{
		float halfDuration = StartingDuration * 0.5f;
		if (_duration > halfDuration)
		{
			float t = 1f - ((_duration - halfDuration) / halfDuration);
			return Mathf.SmoothStep(0.01f, 1f, Mathf.Clamp(t, 0f, 1f));
		}

		float fadeOutT = 1f - (_duration / halfDuration);
		return Mathf.SmoothStep(1f, 0.01f, Mathf.Clamp(fadeOutT, 0f, 1f));
	}

	private static Texture2D CreateMiracleTexture(int size)
	{
		int s = Math.Max(64, size);
		float max = s - 1f;
		float center = max * 0.5f;
		float radius = s * 0.5f;
		var image = Image.CreateEmpty(s, s, false, Image.Format.Rgba8);

		for (int y = 0; y < s; y++)
		{
			for (int x = 0; x < s; x++)
			{
				float nx = (x - center) / radius;
				float ny = (y - center) / radius;
				float radial = Mathf.Clamp(1f - Mathf.Sqrt(nx * nx + ny * ny), 0f, 1f);
				radial = radial * radial * (3f - 2f * radial);

				float diamond = Mathf.Clamp(1f - (Mathf.Abs(nx) + Mathf.Abs(ny)) * 0.82f, 0f, 1f);
				diamond = Mathf.Pow(diamond, 1.85f);

				float horizontal = Mathf.Exp(-Mathf.Abs(nx) * 8.5f) * Mathf.Exp(-Mathf.Abs(ny) * 1.5f);
				float vertical = Mathf.Exp(-Mathf.Abs(ny) * 8.5f) * Mathf.Exp(-Mathf.Abs(nx) * 1.5f);
				float diagonalA = Mathf.Exp(-Mathf.Abs(nx - ny) * 10f) * Mathf.Exp(-(Mathf.Abs(nx) + Mathf.Abs(ny)) * 1.4f);
				float diagonalB = Mathf.Exp(-Mathf.Abs(nx + ny) * 10f) * Mathf.Exp(-(Mathf.Abs(nx) + Mathf.Abs(ny)) * 1.4f);

				float alpha = radial * 0.35f
					+ diamond * 0.55f
					+ Mathf.Max(horizontal, vertical) * 0.28f
					+ Mathf.Max(diagonalA, diagonalB) * 0.14f;

				alpha = Mathf.Clamp(alpha, 0f, 1f);
				image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
			}
		}

		return ImageTexture.CreateFromImage(image);
	}

	private static Vector2? ResolveCreatureCenter(Creature? creature)
	{
		if (creature == null)
		{
			return null;
		}

		if (TestMode.IsOn)
		{
			return null;
		}

		NCreature? nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return nCreature.VfxSpawnPosition;
		}

		Node2D? node = TryGetCreatureNode2D(creature);
		if (node == null)
		{
			return null;
		}

		Marker2D? marker = node.GetNodeOrNull<Marker2D>("%CenterPos") ?? node.GetNodeOrNull<Marker2D>("CenterPos");
		if (marker != null)
		{
			return marker.GlobalPosition;
		}

		return node.GlobalPosition;
	}

	private static Node2D? TryGetCreatureNode2D(Creature creature)
	{
		object instance = creature;
		if (instance is Node2D node2D)
		{
			return node2D;
		}

		Type type = creature.GetType();
		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		foreach (PropertyInfo property in type.GetProperties(Flags))
		{
			if (!typeof(Node).IsAssignableFrom(property.PropertyType))
			{
				continue;
			}

			object? value;
			try
			{
				value = property.GetValue(creature);
			}
			catch
			{
				continue;
			}

			if (value is Node2D propertyNode)
			{
				return propertyNode;
			}
		}

		foreach (FieldInfo field in type.GetFields(Flags))
		{
			if (!typeof(Node).IsAssignableFrom(field.FieldType))
			{
				continue;
			}

			object? value;
			try
			{
				value = field.GetValue(creature);
			}
			catch
			{
				continue;
			}

			if (value is Node2D fieldNode)
			{
				return fieldNode;
			}
		}

		return null;
	}
}
