using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class NSanaeEnergyCounter : NEnergyCounter
{
	private const string DarkenedMatPath = "res://materials/ui/energy_orb_dark.tres";
	private const float FallbackEnergyVfxDurationSeconds = 0.55f;

	private static readonly Color EnergyTint = new Color(0.2f, 1f, 0.8f, 1f);
	private static readonly Color DepletedTint = new Color(0.28f, 0.3f, 0.34f, 1f);

	private Player? _player;
	private Label? _label;
	private Control? _layers;
	private Control? _rotationLayers;
	private Node2D? _energyVfxBack;
	private Node2D? _energyVfxFront;
	private Sprite2D? _backGlow;
	private Sprite2D? _frontGlow;
	private CpuParticles2D? _backParticles;
	private CpuParticles2D? _frontParticles;
	private Material? _darkenedMat;
	private bool _isEnergyZero;
	private readonly Dictionary<CanvasItem, Material?> _baseMaterials = new();

	private float _animationTime;
	private float _energyVfxTimeRemaining;
	private int _lastKnownMaxEnergy;
	private bool _initialized;

	public override void _EnterTree()
	{
	}

	public override void _Ready()
	{
		_player = GetPlayerFromBase();

		_label = GetNodeOrNull<Label>("Label");
		_layers = GetNodeOrNull<Control>("%Layers");
		_rotationLayers = GetNodeOrNull<Control>("%RotationLayers");
		_energyVfxBack = GetNodeOrNull<Node2D>("%EnergyVfxBack");
		_energyVfxFront = GetNodeOrNull<Node2D>("%EnergyVfxFront");
		_backGlow = _energyVfxBack?.GetNodeOrNull<Sprite2D>("BackGlow");
		_frontGlow = _energyVfxFront?.GetNodeOrNull<Sprite2D>("FrontGlow");
		_backParticles = _energyVfxBack?.GetNodeOrNull<CpuParticles2D>("BackParticles");
		_frontParticles = _energyVfxFront?.GetNodeOrNull<CpuParticles2D>("FrontParticles");
		_darkenedMat = GD.Load<Material>(DarkenedMatPath);
		CacheBaseMaterials();

		ApplyEnergyTint(EnergyTint);
		SetEnergyVfxVisible(false);

		_initialized = true;

		if (_player != null)
		{
			CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
			_player.PlayerCombatState.EnergyChanged += OnEnergyChanged;
		}

		RefreshLabelSafe();
	}

	public override void _ExitTree()
	{
		if (_player != null)
		{
			CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
			_player.PlayerCombatState.EnergyChanged -= OnEnergyChanged;
		}
	}

	public override void _Process(double delta)
	{
		if (!_initialized)
		{
			return;
		}

		var deltaSeconds = (float)delta;
		_animationTime += deltaSeconds;
		UpdateRotation(deltaSeconds);

		if (_energyVfxTimeRemaining > 0f)
		{
			_energyVfxTimeRemaining = Mathf.Max(0f, _energyVfxTimeRemaining - deltaSeconds);
			UpdateGlowPulse();

			if (_energyVfxTimeRemaining <= 0f)
			{
				SetEnergyVfxVisible(false);
			}
		}
	}

	private void OnCombatStateChanged(CombatState combatState)
	{
		RefreshLabelSafe();
	}

	private void OnEnergyChanged(int oldEnergy, int newEnergy)
	{
		if (!_initialized)
		{
			return;
		}

		if (newEnergy > oldEnergy)
		{
			TriggerEnergyVfx();
		}

		RefreshLabelSafe();
	}

	private void RefreshLabelSafe()
	{
		if (!_initialized || _player == null || _label == null || _layers == null || _rotationLayers == null)
		{
			return;
		}

		var playerCombatState = _player.PlayerCombatState;
		_isEnergyZero = playerCombatState.Energy == 0;
		_lastKnownMaxEnergy = playerCombatState.MaxEnergy;
		_label.Text = $"{playerCombatState.Energy}/{playerCombatState.MaxEnergy}";

		_layers.Modulate = _isEnergyZero ? Colors.DarkGray : Colors.White;
		_rotationLayers.Modulate = _isEnergyZero ? Colors.DarkGray : Colors.White;
		ApplyEnergyVisualState();
	}

	private void UpdateRotation(float deltaSeconds)
	{
		if (_rotationLayers == null)
		{
			return;
		}

		var baseSpeed = _isEnergyZero ? 7f : 26f;
		for (var i = 0; i < _rotationLayers.GetChildCount(); i++)
		{
			if (_rotationLayers.GetChild(i) is Control c)
			{
				var direction = (i % 2 == 0) ? 1f : -1f;
				var speedMultiplier = 0.55f + (i * 0.35f);
				c.RotationDegrees = Mathf.PosMod(c.RotationDegrees + (deltaSeconds * baseSpeed * speedMultiplier * direction), 360f);
			}
		}
	}

	private static Player? GetPlayerFromBaseField(NEnergyCounter counter)
	{
		var field = typeof(NEnergyCounter).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);
		return field?.GetValue(counter) as Player;
	}

	private Player? GetPlayerFromBase()
	{
		try
		{
			return GetPlayerFromBaseField(this);
		}
		catch
		{
			return null;
		}
	}

	private void ApplyEnergyTint(Color tint)
	{
		if (_energyVfxBack != null)
		{
			_energyVfxBack.Modulate = tint;
		}

		if (_energyVfxFront != null)
		{
			_energyVfxFront.Modulate = tint;
		}

		if (_backGlow != null)
		{
			_backGlow.Modulate = new Color(tint.R, tint.G, tint.B, 0.32f);
		}

		if (_frontGlow != null)
		{
			_frontGlow.Modulate = new Color(tint.R, tint.G, tint.B, 0.2f);
		}

		if (_backParticles != null)
		{
			_backParticles.Color = new Color(tint.R, tint.G, tint.B, 0.22f);
		}

		if (_frontParticles != null)
		{
			_frontParticles.Color = new Color(tint.R, tint.G, tint.B, 0.3f);
		}
	}

	private void ApplyEnergyVisualState()
	{
		if (_layers == null || _rotationLayers == null)
		{
			return;
		}

		foreach (var pair in _baseMaterials)
		{
			pair.Key.Material = _isEnergyZero ? _darkenedMat : pair.Value;
		}

		var tint = _isEnergyZero ? DepletedTint : EnergyTint;
		ApplyEnergyTint(tint);

		if (_label != null)
		{
			_label.Modulate = Colors.White;
		}

		if (_energyVfxTimeRemaining > 0f)
		{
			UpdateGlowPulse();
		}
	}

	private void UpdateGlowPulse()
	{
		UpdateGlow(_backGlow, 0.5f, 0.3f, 0.04f, 0.32f, 0.12f, 1.8f, 0f);
		UpdateGlow(_frontGlow, 0.43f, 0.26f, 0.03f, 0.2f, 0.08f, 2.3f, 0.85f);
	}

	private void UpdateGlow(Sprite2D? glow, float baseScale, float scaleAmplitude, float depletedScaleAmplitude, float baseAlpha, float alphaAmplitude, float pulseSpeed, float phase)
	{
		if (glow == null)
		{
			return;
		}

		var pulse = (Mathf.Sin((_animationTime * pulseSpeed) + phase) + 1f) * 0.5f;
		var activeScaleAmplitude = _isEnergyZero ? depletedScaleAmplitude : scaleAmplitude;
		var alpha = _isEnergyZero
			? baseAlpha * 0.45f
			: baseAlpha + (pulse * alphaAmplitude);

		glow.Scale = Vector2.One * (baseScale + (pulse * activeScaleAmplitude));
		glow.Modulate = new Color(glow.Modulate.R, glow.Modulate.G, glow.Modulate.B, Mathf.Clamp(alpha, 0f, 1f));
	}

	private void CacheBaseMaterials()
	{
		_baseMaterials.Clear();
		RememberBaseMaterials(_layers);
		RememberBaseMaterials(_rotationLayers);
		RememberBaseMaterials(_energyVfxBack);
		RememberBaseMaterials(_energyVfxFront);
	}

	private void RememberBaseMaterials(Node? node)
	{
		if (node == null)
		{
			return;
		}

		if (node is CanvasItem canvasItem && canvasItem != _label && !_baseMaterials.ContainsKey(canvasItem))
		{
			_baseMaterials[canvasItem] = canvasItem.Material;
		}

		for (var i = 0; i < node.GetChildCount(); i++)
		{
			RememberBaseMaterials(node.GetChild(i));
		}
	}

	private void TriggerEnergyVfx()
	{
		_animationTime = 0f;
		_energyVfxTimeRemaining = GetEnergyVfxDurationSeconds();
		SetEnergyVfxVisible(true);
		ResetEnergyVfxVisuals();
		UpdateGlowPulse();

		if (_backParticles != null)
		{
			_backParticles.Emitting = false;
			_backParticles.Restart();
			_backParticles.Emitting = true;
		}

		if (_frontParticles != null)
		{
			_frontParticles.Emitting = false;
			_frontParticles.Restart();
			_frontParticles.Emitting = true;
		}
	}

	private void ResetEnergyVfxVisuals()
	{
		ResetGlow(_backGlow, 0.5f, 0.32f);
		ResetGlow(_frontGlow, 0.43f, 0.2f);
	}

	private static void ResetGlow(Sprite2D? glow, float baseScale, float baseAlpha)
	{
		if (glow == null)
		{
			return;
		}

		glow.Scale = Vector2.One * baseScale;
		glow.Modulate = new Color(glow.Modulate.R, glow.Modulate.G, glow.Modulate.B, baseAlpha);
	}

	private float GetEnergyVfxDurationSeconds()
	{
		float backLifetime = (float)(_backParticles?.Lifetime ?? 0.0);
		float frontLifetime = (float)(_frontParticles?.Lifetime ?? 0.0);
		float duration = Mathf.Max(backLifetime, frontLifetime);
		return duration > 0f ? duration : FallbackEnergyVfxDurationSeconds;
	}

	private void SetEnergyVfxVisible(bool visible)
	{
		if (_energyVfxBack != null)
		{
			_energyVfxBack.Visible = visible;
		}

		if (_energyVfxFront != null)
		{
			_energyVfxFront.Visible = visible;
		}

		if (_backParticles != null)
		{
			_backParticles.Emitting = visible;
		}

		if (_frontParticles != null)
		{
			_frontParticles.Emitting = visible;
		}

		if (!visible)
		{
			_energyVfxTimeRemaining = 0f;
		}
	}
}
