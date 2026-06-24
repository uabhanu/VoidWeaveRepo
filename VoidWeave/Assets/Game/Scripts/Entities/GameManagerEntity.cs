namespace Game.Scripts.Entities
{
    using Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class GameManagerEntity : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int beamTurretUnlockLevel;
        [SerializeField] private float boundaryOffset;
        [SerializeField] private float bulletRotationOffset; // PI / 2
        [SerializeField] private float cameraOrthographicSize;
        [SerializeField] private float damageMultiplierPerLevel;
        [SerializeField] private float eliteStatMultiplier;
        [SerializeField] private float floatTolerance;
        [SerializeField] private int lastLevel;
        [SerializeField] private int maxTurretLevel;
        [SerializeField] private float normalStatMultiplier;
        [SerializeField] private int enemiesToKill;
        [SerializeField] private int enemiesToKillIncrement;
        [SerializeField] private GameObject enemySpawnerEntityPrefab;
        [SerializeField] private float healthMultiplierPerLevel;
        [SerializeField] private float healthValueForDeath;
        [SerializeField] private GameObject inputEntityPrefab;
        [SerializeField] private int level1EnergyForTutorial;
        [SerializeField] private int level2EnergyForTutorial;
        [SerializeField] private int level3EnergyForTutorial;
        [SerializeField] private int lineEnemyUnlockLevel; //The When
        [SerializeField] private float lootMultiplierPerLevel;
        [SerializeField] private float lootPickupRadius;
        [SerializeField] private int lootSpawnedFirstTime;
        [SerializeField] private int mainMenuState;
        [SerializeField] private int maxLevelForTutorials;
        [SerializeField] private int minEnemiesToKill;
        [SerializeField] private float minOverlapDistance;
        [SerializeField] private int minStartingLevel;
        [SerializeField] private GameObject playerEntityPrefab;
        [SerializeField] private int playingState;
        [SerializeField] private float scalingBase;
        [SerializeField] private int scalingLevelOffset;
        [SerializeField] private int scalingMinLevel;
        [SerializeField] private int scatterTurretUnlockLevel;
        [SerializeField] private float separationDistance;
        [SerializeField] private float separationVelocity;
        [SerializeField] private int squareEnemyUnlockLevel; //The When
        [SerializeField] private int startingLevel;
        [Tooltip("Chosen this high value on purpose to make turret shoot nothing when there is no target")] [SerializeField] private float targetDefaultPosition;
        [SerializeField] private float timerExpired;
        [SerializeField] private int triangleEnemyUnlockLevel; //The When
        [SerializeField] private GameObject turretConfigEntityPrefab;
        [SerializeField] private uint unlockedLineEnemy; //The What
        [SerializeField] private uint unlockedSquareEnemy; //The What
        [SerializeField] private uint unlockedTriangleEnemy; //The What
        [SerializeField] private float upgradeCostBaseMultiplier;
        [SerializeField] private float upgradeCostMultiplier;
        [SerializeField] private float wave1Multiplier;
        [SerializeField] private float wave2Multiplier;
        [SerializeField] private float wave3Multiplier;
        [SerializeField] private int wavesPerLevel;
        [SerializeField] private int waveStateCombat;
        [SerializeField] private int waveStatePrep;

        #endregion

        #region Baker

        private class GameManagerBaker : Baker<GameManagerEntity>
        {
            public override void Bake(GameManagerEntity authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                int enemiesToKill = math.max(authoring.minEnemiesToKill , authoring.enemiesToKill);
                int startingLevel = math.max(authoring.minStartingLevel , authoring.startingLevel);

                uint initialMask = 0;
                initialMask |= math.select(0 , authoring.unlockedLineEnemy , startingLevel >= authoring.lineEnemyUnlockLevel);
                initialMask |= math.select(0 , authoring.unlockedTriangleEnemy , startingLevel >= authoring.triangleEnemyUnlockLevel);
                initialMask |= math.select(0 , authoring.unlockedSquareEnemy , startingLevel >= authoring.squareEnemyUnlockLevel);

                AddComponent(entity , new BeamTurretUnlockLevelComponent { Value = authoring.beamTurretUnlockLevel });
                AddComponent(entity , new BoundaryOffsetComponent { Value = authoring.boundaryOffset });
                AddComponent(entity , new CameraOrthographicSizeComponent { Value = authoring.cameraOrthographicSize });
                AddComponent(entity , new CurrentEnergyComponent { Value = authoring.level1EnergyForTutorial });
                AddComponent(entity , new DamageMultiplierComponent { Value = authoring.damageMultiplierPerLevel });
                AddComponent(entity , new EliteStatMultiplierComponent { Value = authoring.eliteStatMultiplier });
                AddComponent(entity , new EnemiesToKillComponent { Value = enemiesToKill });
                AddComponent(entity , new EnemiesKilledComponent());
                AddComponent(entity , new EnemiesToKillIncrementComponent { Value = authoring.enemiesToKillIncrement });
                AddComponent(entity , new EnemySpawnerEntityComponent { Entity = GetEntity(authoring.enemySpawnerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new FloatToleranceComponent { Value = authoring.floatTolerance });
                AddComponent(entity , new GameStateComponent { Value = authoring.mainMenuState });
                AddComponent(entity , new HealthMultiplierComponent { Value = authoring.healthMultiplierPerLevel });
                AddComponent(entity , new HealthValueForDeathComponent { Value = authoring.healthValueForDeath });
                AddComponent(entity , new InitializeGameTag());
                AddComponent(entity , new InputEntityComponent { Entity = GetEntity(authoring.inputEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new LastLevelComponent { Value = authoring.lastLevel });
                AddComponent(entity , new LevelComponent { Value = authoring.startingLevel });
                AddComponent(entity , new Level1EnergyForTutorialComponent { Value = authoring.level1EnergyForTutorial });
                AddComponent(entity , new Level2EnergyForTutorialComponent { Value = authoring.level2EnergyForTutorial });
                AddComponent(entity , new Level3EnergyForTutorialComponent { Value = authoring.level3EnergyForTutorial });
                AddComponent(entity , new LevelToUnlockLineEnemyComponent { Value = authoring.lineEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockSquareEnemyComponent { Value = authoring.squareEnemyUnlockLevel });
                AddComponent(entity , new LevelToUnlockTriangleEnemyComponent { Value = authoring.triangleEnemyUnlockLevel });
                AddComponent(entity , new LootMultiplierComponent { Value = authoring.lootMultiplierPerLevel });
                AddComponent(entity , new LootPickupRadiusComponent { Value = authoring.lootPickupRadius });
                AddComponent(entity , new LootSpawnedFirstTimeComponent { Value = authoring.lootSpawnedFirstTime });
                AddComponent(entity , new MainMenuStateComponent { Value = authoring.mainMenuState });
                AddComponent(entity , new MaxLevelsForTutorialsComponent { Value = authoring.maxLevelForTutorials });
                AddComponent(entity , new MaxTurretLevelComponent { Value = authoring.maxTurretLevel });
                AddComponent(entity , new MinOverlapDistanceComponent { Value = authoring.minOverlapDistance });
                AddComponent(entity , new NormalStatMultiplierComponent { Value = authoring.normalStatMultiplier });
                AddComponent(entity , new PlayerEntityComponent { Entity = GetEntity(authoring.playerEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new PlayingStateComponent { Value = authoring.playingState });
                AddComponent(entity , new ScalingBaseComponent { Value = authoring.scalingBase });
                AddComponent(entity , new ScalingLevelOffsetComponent { Value = authoring.scalingLevelOffset });
                AddComponent(entity , new ScalingMinLevelComponent { Value = authoring.scalingMinLevel });
                AddComponent(entity , new ScatterTurretUnlockLevelComponent { Value = authoring.scatterTurretUnlockLevel });
                AddComponent(entity , new SeparationDistanceComponent { Value = authoring.separationDistance });
                AddComponent(entity , new SeparationVelocityComponent { Value = authoring.separationVelocity });
                AddComponent(entity , new TargetDefaultPositionComponent { Value = authoring.targetDefaultPosition });
                AddComponent(entity , new TimerExpiredComponent { Value = authoring.timerExpired });
                AddComponent(entity , new TurretConfigEntityComponent { Entity = GetEntity(authoring.turretConfigEntityPrefab , TransformUsageFlags.Dynamic) });
                AddComponent(entity , new UnlockedEnemiesComponent { Value = initialMask });
                AddComponent(entity , new UnlockedLineEnemyComponent { Value = authoring.unlockedLineEnemy });
                AddComponent(entity , new UnlockedSquareEnemyComponent { Value = authoring.unlockedSquareEnemy });
                AddComponent(entity , new UnlockedTriangleEnemyComponent { Value = authoring.unlockedTriangleEnemy });
                AddComponent(entity , new UpgradeCostBaseMultiplierComponent { Value = authoring.upgradeCostBaseMultiplier });
                AddComponent(entity , new UpgradeCostMultiplierComponent { Value = authoring.upgradeCostMultiplier });
                AddComponent(entity , new Wave1MultiplierComponent { Value = authoring.wave1Multiplier });
                AddComponent(entity , new Wave2MultiplierComponent { Value = authoring.wave2Multiplier });
                AddComponent(entity , new Wave3MultiplierComponent { Value = authoring.wave3Multiplier });
                AddComponent(entity , new WavesPerLevelComponent { Value = authoring.wavesPerLevel });
                AddComponent(entity , new WaveStateCombatComponent { Value = authoring.waveStateCombat });
                AddComponent(entity , new WaveStatePrepComponent { Value = authoring.waveStatePrep });
                
                SetComponentEnabled<InitializeGameTag>(entity , false);
            }
        }

        #endregion
    }
}