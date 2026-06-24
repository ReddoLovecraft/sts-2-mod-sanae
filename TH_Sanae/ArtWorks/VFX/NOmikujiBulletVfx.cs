using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;
using System;
using System.Collections.Generic;
using TH_Sanae.Scripts.Patches;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

public partial class NOmikujiBulletVfx : Node2D
{
	private const string AdditiveMatPath = "res://TH_Sanae/ArtWorks/VFX/canvas_item_material_additive_shared.tres";
	private const string BulletTexPath = "res://TH_Sanae/ArtWorks/VFX/bulletCe000.png";
	private const string LuckTexPath = "res://TH_Sanae/ArtWorks/VFX/bulletCd000.png";
	private const string BadLuckTexPath = "res://TH_Sanae/ArtWorks/VFX/bulletCd001.png";
	private const string GreatLuckTexPath = "res://TH_Sanae/ArtWorks/VFX/bulletCd002.png";
	private const string GreatCurseTexPath = "res://TH_Sanae/ArtWorks/VFX/bulletCd003.png";

	private static Material? _additiveMat;
	private static Texture2D? _bulletTex;
	private static readonly Dictionary<DrawResultType, Texture2D?> ResultTextures = new();

	private Vector2 _start;
	private Vector2 _end;
	private Vector2 _control;
	private DrawResultType _result;

	private float _elapsed;
	private float _impactElapsed;

	private Sprite2D? _bulletSprite;
	private Sprite2D? _impactSprite;
	private Sprite2D? _textSprite;

	public float TravelDurationSeconds { get; set; } = 0.35f;
	public float ImpactDurationSeconds { get; set; } = 0.45f;
	public float ArcHeightPixels { get; set; } = 160f;
	public Vector2 TextOffsetPixels { get; set; } = new Vector2(0f, -54f);

	internal static NOmikujiBulletVfx? Create(Vector2 start, Vector2 end, DrawResultType result)
	{
		if (TestMode.IsOn)
		{
			return null;
		}

		var vfx = new NOmikujiBulletVfx();
		vfx._start = start;
		vfx._end = end;
		vfx._result = result;
		return vfx;
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		TopLevel = true;
		ZAsRelative = false;
		ZIndex = 45;

		_additiveMat ??= ResourceLoader.Load<Material>(AdditiveMatPath);
		_bulletTex ??= ResourceLoader.Load<Texture2D>(BulletTexPath);
		if (ResultTextures.Count == 0)
		{
			ResultTextures[DrawResultType.GreatLuck] = ResourceLoader.Load<Texture2D>(GreatLuckTexPath);
			ResultTextures[DrawResultType.Luck] = ResourceLoader.Load<Texture2D>(LuckTexPath);
			ResultTextures[DrawResultType.BadLuck] = ResourceLoader.Load<Texture2D>(BadLuckTexPath);
			ResultTextures[DrawResultType.GreatCurse] = ResourceLoader.Load<Texture2D>(GreatCurseTexPath);
		}

		_bulletSprite = new Sprite2D
		{
			Name = "Bullet",
			Centered = true,
			Texture = _bulletTex,
			Material = _additiveMat,
			Scale = Vector2.One * 0.75f
		};
		AddChild(_bulletSprite);

		_impactSprite = new Sprite2D
		{
			Name = "Impact",
			Centered = true,
			Texture = _bulletTex,
			Material = _additiveMat,
			Visible = false
		};
		AddChild(_impactSprite);

		_textSprite = new Sprite2D
		{
			Name = "ResultText",
			Centered = true,
			Material = _additiveMat,
			Visible = false
		};
		AddChild(_textSprite);

		_control = ResolveControlPoint(_start, _end);
		UpdateTravel(0f);
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		if (_impactSprite != null && _impactSprite.Visible)
		{
			_impactElapsed += dt;
			UpdateImpact(Mathf.Clamp(_impactElapsed / Math.Max(0.001f, ImpactDurationSeconds), 0f, 1f));
			if (_impactElapsed >= ImpactDurationSeconds)
			{
				QueueFree();
			}
			return;
		}

		_elapsed += dt;
		float t = Mathf.Clamp(_elapsed / Math.Max(0.001f, TravelDurationSeconds), 0f, 1f);
		UpdateTravel(t);
		if (t >= 1f)
		{
			SwitchToImpact();
		}
	}

	private void UpdateTravel(float t)
	{
		if (_bulletSprite == null)
		{
			return;
		}

		Vector2 pos = Bezier2(_start, _control, _end, EaseOut(t));
		Vector2 vel = Bezier2Derivative(_start, _control, _end, EaseOut(t));

		_bulletSprite.GlobalPosition = pos;
		if (vel.LengthSquared() > 0.0001f)
		{
			_bulletSprite.Rotation = vel.Angle();
		}

		float s = Mathf.Lerp(0.72f, 0.92f, t);
		_bulletSprite.Scale = Vector2.One * s;
	}

	private void SwitchToImpact()
	{
		if (_bulletSprite != null)
		{
			_bulletSprite.Visible = false;
		}

		if (_impactSprite != null)
		{
			_impactSprite.Visible = true;
			_impactSprite.GlobalPosition = _end;
			_impactSprite.Rotation = 0f;
		}

		if (_textSprite != null)
		{
			_textSprite.Visible = true;
			_textSprite.GlobalPosition = _end + TextOffsetPixels;
			_textSprite.Texture = ResultTextures.TryGetValue(_result, out Texture2D? tex) ? tex : null;
			_textSprite.Modulate = ResolveResultColor(_result);
		}

		_impactElapsed = 0f;
		UpdateImpact(0f);
	}

	private void UpdateImpact(float t)
	{
		if (_impactSprite != null)
		{
			float tt = EaseOut(t);
			float scale = Mathf.Lerp(0.85f, 2.05f, tt);
			Color c = Colors.White;
			c.A = 1f - tt;
			_impactSprite.Scale = Vector2.One * scale;
			_impactSprite.Modulate = c;
		}

		if (_textSprite != null)
		{
			float fadeIn = Mathf.Clamp(t / 0.12f, 0f, 1f);
			float fadeOut = t < 0.25f ? 1f : Mathf.Clamp(1f - ((t - 0.25f) / 0.75f), 0f, 1f);
			float a = fadeIn * fadeOut;
			Color m = _textSprite.Modulate;
			m.A = a;
			_textSprite.Modulate = m;

			float rise = Mathf.Lerp(0f, -18f, EaseOut(t));
			_textSprite.GlobalPosition = _end + TextOffsetPixels + new Vector2(0f, rise);
			_textSprite.Scale = Vector2.One * Mathf.Lerp(0.85f, 1.05f, EaseOut(t));
		}
	}

	private Vector2 ResolveControlPoint(Vector2 start, Vector2 end)
	{
		float dist = start.DistanceTo(end);
		float height = Mathf.Clamp(ArcHeightPixels, 60f, 260f);
		height = Mathf.Min(height, dist * 0.55f + 60f);
		return (start + end) * 0.5f + new Vector2(0f, -height);
	}

	private static Color ResolveResultColor(DrawResultType result)
	{
		Color c = result switch
		{
			DrawResultType.GreatLuck => StsColors.gold,
			DrawResultType.Luck => StsColors.green,
			DrawResultType.BadLuck => StsColors.red,
			_ => StsColors.purple
		};
		c.A = 1f;
		return c;
	}

	private static float EaseOut(float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		float u = 1f - t;
		return 1f - u * u * u;
	}

	private static Vector2 Bezier2(Vector2 a, Vector2 b, Vector2 c, float t)
	{
		float u = 1f - t;
		return (u * u) * a + (2f * u * t) * b + (t * t) * c;
	}

	private static Vector2 Bezier2Derivative(Vector2 a, Vector2 b, Vector2 c, float t)
	{
		return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
	}
}
