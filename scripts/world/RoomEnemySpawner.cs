using Godot;
using System;
using System.Collections.Generic;

public partial class RoomEnemySpawner : Node
{
	[Export] public PackedScene EnemyScene { get; set; } = GD.Load<PackedScene>("res://scenes/enemies/Enemy.tscn");

	private List<Node> _activeEnemies = new List<Node>();

	public event Action OnAllEnemiesDefeated;

	public void SpawnEnemies(Node2D parent)
	{
		if (EnemyScene == null) return;

		_activeEnemies.Clear();
		Vector2 screenSize = parent.GetViewportRect().Size;
		var random = new RandomNumberGenerator();
		random.Randomize();

		int enemiesToSpawn = 3;

		for (int i = 0; i < enemiesToSpawn; i++)
		{
			Node2D enemyInstance = EnemyScene.Instantiate<Node2D>();
			parent.AddChild(enemyInstance);

			enemyInstance.TreeExited += OnEnemyDefeated;
			_activeEnemies.Add(enemyInstance);

			float randomX = random.RandfRange(150.0f, screenSize.X - 150.0f);
			float randomY = random.RandfRange(150.0f, screenSize.Y - 150.0f);

			enemyInstance.GlobalPosition = new Vector2(randomX, randomY);
		}
	}

	public int GetActiveEnemyCount()
	{
		_activeEnemies.RemoveAll(e => !GodotObject.IsInstanceValid(e) || !e.IsInsideTree());
		return _activeEnemies.Count;
	}

	public void RemoveEnemy(Node enemy)
	{
		_activeEnemies.Remove(enemy);
		if (_activeEnemies.Count == 0)
		{
			OnAllEnemiesDefeated?.Invoke();
		}
	}

	private void OnEnemyDefeated()
	{
		CallDeferred(nameof(CheckEnemiesCount));
	}

	private void CheckEnemiesCount()
	{
		_activeEnemies.RemoveAll(e => !GodotObject.IsInstanceValid(e) || !e.IsInsideTree());

		GD.Print($"[RoomEnemySpawner] Активных врагов: {_activeEnemies.Count}");

		if (_activeEnemies.Count == 0)
		{
			OnAllEnemiesDefeated?.Invoke();
		}
	}
}
