using Core;
using NUnit.Framework;

public class RoomStateTests
{
    [Test]
    public void RoomCombatState_ActivatesWithEnemyCount()
    {
        var state = new RoomCombatState();

        state.Activate(3);

        Assert.That(state.IsActive, Is.True);
        Assert.That(state.ActiveEnemyCount, Is.EqualTo(3));
    }

    [Test]
    public void RoomCombatState_BecomesClearedAfterAllEnemiesDefeated()
    {
        var state = new RoomCombatState();
        state.Activate(2);

        state.NotifyEnemyDefeated();
        state.NotifyEnemyDefeated();

        Assert.That(state.ActiveEnemyCount, Is.EqualTo(0));
        Assert.That(state.IsActive, Is.True);
    }

    [Test]
    public void RoomCombatState_ResetClearsActivation()
    {
        var state = new RoomCombatState();
        state.Activate(1);

        state.Reset();

        Assert.That(state.IsActive, Is.False);
        Assert.That(state.ActiveEnemyCount, Is.EqualTo(0));
    }

    [Test]
    public void RoomLifecycle_MarksRewardOnlyOnce()
    {
        var lifecycle = new RoomLifecycle();

        lifecycle.MarkRewardSpawned();
        lifecycle.MarkRewardSpawned();

        Assert.That(lifecycle.RewardSpawned, Is.True);
    }

    [Test]
    public void RunState_ResetClearsRoomProgress()
    {
        var state = new Core.RunState();
        state.ClearedRooms.Add("ROOM_01");
        state.RoomsWithPendingReward.Add("ROOM_01");
        state.RoomsRewardCollected.Add("ROOM_00");
        state.RoomRewardIndices["ROOM_01"] = 1;

        state.Reset();

        Assert.That(state.ClearedRooms, Is.Empty);
        Assert.That(state.RoomsWithPendingReward, Is.Empty);
        Assert.That(state.RoomsRewardCollected, Is.Empty);
        Assert.That(state.RoomRewardIndices, Is.Empty);
    }

    [Test]
    public void RunState_StoresRoomConnectionsAndRewards()
    {
        var state = new Core.RunState();
        state.RoomConnections["ROOM_00|Right"] = "ROOM_01";
        state.RoomRewardIndices["ROOM_01"] = 0;

        Assert.That(state.RoomConnections["ROOM_00|Right"], Is.EqualTo("ROOM_01"));
        Assert.That(state.RoomRewardIndices["ROOM_01"], Is.EqualTo(0));
    }
}