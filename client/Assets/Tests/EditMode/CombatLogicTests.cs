using NUnit.Framework;
using UnityEngine;
using EmberBlade.Combat;
using EmberBlade.Config;
using EmberBlade.Missions;

public class CombatLogicTests
{
    [Test]
    public void HealthDamageRaisesDeathOnce()
    {
        GameObject objectUnderTest = new GameObject("HealthTest");
        try
        {
            Health health = objectUnderTest.AddComponent<Health>();
            int damageEvents = 0;
            int deathEvents = 0;
            health.OnDamaged += (_, _) => damageEvents++;
            health.OnDied += () => deathEvents++;
            health.Initialize(100);

            health.Damage(30);
            health.Damage(100);
            health.Damage(10);

            Assert.AreEqual(0, health.CurrentHealth);
            Assert.AreEqual(2, damageEvents);
            Assert.AreEqual(1, deathEvents);
        }
        finally
        {
            Object.DestroyImmediate(objectUnderTest);
        }
    }

    [Test]
    public void EmberMeterClampsAndSpendsAtMaximum()
    {
        GameObject objectUnderTest = new GameObject("EmberTest");
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        try
        {
            config.emberMax = 100;
            EmberMeter ember = objectUnderTest.AddComponent<EmberMeter>();
            ember.Initialize(config);
            ember.Add(140);

            Assert.AreEqual(100, ember.Value);
            Assert.IsTrue(ember.TrySpendEmpowered());
            Assert.AreEqual(0, ember.Value);
            Assert.IsFalse(ember.TrySpendEmpowered());
        }
        finally
        {
            Object.DestroyImmediate(objectUnderTest);
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void BarricadeLimitsUseConfiguredMaximums()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        try
        {
            config.maxBarricadeWalls = 8;
            config.maxBarricadeTraps = 3;
            Assert.IsTrue(BarricadeManager.CanPlaceWithinLimit(
                BarricadePieceType.Wall, 7, 3, config));
            Assert.IsFalse(BarricadeManager.CanPlaceWithinLimit(
                BarricadePieceType.Wall, 8, 0, config));
            Assert.IsTrue(BarricadeManager.CanPlaceWithinLimit(
                BarricadePieceType.SpikeTrap, 8, 2, config));
            Assert.IsFalse(BarricadeManager.CanPlaceWithinLimit(
                BarricadePieceType.SpikeTrap, 0, 3, config));
        }
        finally
        {
            Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void MissionSequenceAdvancesChapterOneInOrder()
    {
        Assert.AreEqual("1.2", MissionSequence.Next("1.1"));
        Assert.AreEqual("1.3", MissionSequence.Next("1.2"));
        Assert.AreEqual(string.Empty, MissionSequence.Next("1.3"));
    }
}
