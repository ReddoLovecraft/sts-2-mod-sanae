using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

public partial class NSanaeCircleBackVfx : Node2D
{
	private const string AdditiveMatPath = "res://TH_Sanae/ArtWorks/VFX/canvas_item_material_additive_shared.tres";
	private const string FramesDir = "res://TH_Sanae/ArtWorks/VFX/vfx_circleback";

	private static readonly Dictionary<string, Texture2D[]> FramesCache = new(StringComparer.Ordinal);
	private static Material? _additiveMat;

	[Export] public float Speed { get; set; } = 1f;
	[Export] public float Fps { get; set; } = 24f;
	[Export] public float FrontOffsetPx { get; set; } = 140f;
	[Export] public float PairOffsetPx { get; set; } = 44f;

	private Sprite2D? _spriteA;
	private Sprite2D? _spriteB;
	private Texture2D[] _frames = Array.Empty<Texture2D>();
	private float _animTimeScaledSeconds;
	private float _animTotalScaledSeconds;

	private Creature? _source;
	private Creature? _target;
	private Vector2 _startPos;
	private Vector2 _endPos;
	private bool _hasInit;

	public static NSanaeCircleBackVfx? Create(Creature? source, Creature? target, float speed = 1f)
	{
		PackedScene? ps = ResourceLoader.Load<PackedScene>("res://TH_Sanae/ArtWorks/VFX/vfx_circleback.tscn");
		if (ps == null)
		{
			return null;
		}

		var inst = ps.Instantiate<Node>();
		if (inst is not NSanaeCircleBackVfx vfx)
		{
			inst.QueueFree();
			return null;
		}

		vfx._source = source;
		vfx._target = target;
		vfx.Speed = speed;
		return vfx;
	}

	public override void _Ready()
	{
		_spriteA = GetNodeOrNull<Sprite2D>("SpriteA");
		_spriteB = GetNodeOrNull<Sprite2D>("SpriteB");

		if (_spriteA == null)
		{
			_spriteA = new Sprite2D { Name = "SpriteA", Centered = true };
			AddChild(_spriteA);
		}

		if (_spriteB == null)
		{
			_spriteB = new Sprite2D { Name = "SpriteB", Centered = true };
			AddChild(_spriteB);
		}

		_additiveMat ??= ResourceLoader.Load<Material>(AdditiveMatPath);
		if (_additiveMat != null)
		{
			_spriteA.Material = _additiveMat;
			_spriteB.Material = _additiveMat;
		}

		_frames = LoadFrames(FramesDir);
		_animTotalScaledSeconds = _frames.Length <= 0 ? 0f : _frames.Length / Mathf.Max(1f, Fps);

		InitIfNeeded();
		UpdateFrame(0);
	}

	public override void _Process(double delta)
	{
		if (_frames.Length <= 0 || _spriteA == null || _spriteB == null)
		{
			QueueFree();
			return;
		}

		InitIfNeeded();

		float d = (float)delta;
		float effectiveSpeed = Mathf.Max(0.001f, Speed);
		_animTimeScaledSeconds += d * effectiveSpeed;

		int frameIndex = Mathf.FloorToInt(_animTimeScaledSeconds * Mathf.Max(1f, Fps));
		if (frameIndex >= _frames.Length)
		{
			QueueFree();
			return;
		}

		UpdateFrame(frameIndex);

		float p = _animTotalScaledSeconds <= 0f ? 1f : Mathf.Clamp(_animTimeScaledSeconds / _animTotalScaledSeconds, 0f, 1f);
		float t = p <= 0.5f ? (p * 2f) : (2f - p * 2f);
		Vector2 basePos = _startPos.Lerp(_endPos, t);

		_spriteA.GlobalPosition = basePos + new Vector2(0f, -PairOffsetPx);
		_spriteB.GlobalPosition = basePos + new Vector2(0f, PairOffsetPx);
	}

	private void InitIfNeeded()
	{
		if (_hasInit)
		{
			return;
		}

		Vector2 sourceCenter = ResolveCreatureCenter(_source) ?? GlobalPosition;
		_startPos = sourceCenter + ResolveFrontOffset(_source, FrontOffsetPx);
		_endPos = ResolveCreatureCenter(_target) ?? _startPos;
		_hasInit = true;
	}

	private void UpdateFrame(int frameIndex)
	{
		Texture2D tex = _frames[Mathf.Clamp(frameIndex, 0, _frames.Length - 1)];
		if (_spriteA != null && !ReferenceEquals(_spriteA.Texture, tex))
		{
			_spriteA.Texture = tex;
		}
		if (_spriteB != null && !ReferenceEquals(_spriteB.Texture, tex))
		{
			_spriteB.Texture = tex;
		}
	}

	private static Vector2 ResolveFrontOffset(Creature? creature, float amountPx)
	{
		if (creature == null)
		{
			return new Vector2(amountPx, 0f);
		}

		try
		{
			float sign = creature.Side == CombatSide.Player ? 1f : -1f;
			return new Vector2(sign * amountPx, 0f);
		}
		catch
		{
			return new Vector2(amountPx, 0f);
		}
	}

	private static Vector2? ResolveCreatureCenter(Creature? creature)
	{
		if (creature == null)
		{
			return null;
		}

		Node2D? n = TryGetCreatureNode2D(creature);
		if (n == null)
		{
			return null;
		}

		Marker2D? marker = n.GetNodeOrNull<Marker2D>("%CenterPos") ?? n.GetNodeOrNull<Marker2D>("CenterPos");
		if (marker != null)
		{
			return marker.GlobalPosition;
		}

		return n.GlobalPosition;
	}

	private static Node2D? TryGetCreatureNode2D(Creature creature)
	{
		object o = creature;
		if (o is Node2D n2d)
		{
			return n2d;
		}

		if (o is Node node && node is Node2D n2d2)
		{
			return n2d2;
		}

		Type t = creature.GetType();
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		foreach (PropertyInfo p in t.GetProperties(flags))
		{
			if (!typeof(Node).IsAssignableFrom(p.PropertyType))
			{
				continue;
			}

			object? v;
			try
			{
				v = p.GetValue(creature);
			}
			catch
			{
				continue;
			}

			if (v is Node2D n)
			{
				return n;
			}
		}

		foreach (FieldInfo f in t.GetFields(flags))
		{
			if (!typeof(Node).IsAssignableFrom(f.FieldType))
			{
				continue;
			}

			object? v;
			try
			{
				v = f.GetValue(creature);
			}
			catch
			{
				continue;
			}

			if (v is Node2D n)
			{
				return n;
			}
		}

		return null;
	}

	private static Texture2D[] LoadFrames(string framesDir)
	{
		if (FramesCache.TryGetValue(framesDir, out Texture2D[]? cached))
		{
			return cached;
		}

		var textures = new List<Texture2D>();
		DirAccess? dir = DirAccess.Open(framesDir);
		if (dir == null)
		{
			FramesCache[framesDir] = Array.Empty<Texture2D>();
			return FramesCache[framesDir];
		}

		string[] files = dir.GetFiles().Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f, StringComparer.Ordinal).ToArray();
		foreach (string f in files)
		{
			string path = $"{framesDir}/{f}";
			Texture2D? t = ResourceLoader.Load<Texture2D>(path);
			if (t != null)
			{
				textures.Add(t);
			}
		}

		FramesCache[framesDir] = textures.ToArray();
		return FramesCache[framesDir];
	}
}
